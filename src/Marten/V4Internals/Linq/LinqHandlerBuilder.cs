using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Baseline;
using LamarCodeGeneration;
using Marten.Linq;
using Marten.Util;
using Marten.V4Internals.Linq.Includes;
using Marten.V4Internals.Linq.QueryHandlers;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Marten.V4Internals.Linq
{
    public class LinqHandlerBuilder
    {
        private readonly IMartenSession _session;

        public LinqHandlerBuilder(IMartenSession session, Expression expression, ResultOperatorBase additionalOperator = null)
        {
            _session = session;
            Model = MartenQueryParser.TransformQueryFlyweight.GetParsedQuery(expression);
            if (additionalOperator != null) Model.ResultOperators.Add(additionalOperator);

            var storage = session.StorageFor(Model.SourceType());
            TopStatement = CurrentStatement = new DocumentStatement(storage);


            // TODO -- this probably needs to get fancier later
            if (Model.MainFromClause.FromExpression is SubQueryExpression sub)
            {
                processQueryModel(sub.QueryModel, storage, true);
                processQueryModel(Model, storage, false);
            }
            else
            {
                processQueryModel(Model, storage, true);
            }


        }

        private void handleSelector()
        {
            // Important to deal with the selector first before you go into
            // the result operators
            switch (Model.SelectClause.Selector.NodeType)
            {
                case ExpressionType.MemberAccess:
                    CurrentStatement.ToScalar(Model.SelectClause.Selector);
                    break;

                case ExpressionType.Call:
                    var method = (MethodCallExpression)Model.SelectClause.Selector;
                    if (method.Method.Name == nameof(CompiledQueryExtensions.AsJson))
                    {
                        CurrentStatement.ToJsonSelector();
                    }
                    else
                    {
                        throw new NotImplementedException($"Marten does not (yet) support the {method.Method.DeclaringType.FullNameInCode()}.{method.Method.Name}() method as a Linq selector");
                    }

                    break;

                case ExpressionType.MemberInit:
                case ExpressionType.New:
                    CurrentStatement.ToSelectTransform(Model.SelectClause);
                    break;
            }
        }

        private void processQueryModel(QueryModel queryModel, IDocumentStorage storage, bool considerSelectors)
        {
            for (var i = 0; i < queryModel.BodyClauses.Count; i++)
            {
                var clause = queryModel.BodyClauses[i];
                switch (clause)
                {
                    case WhereClause where:
                        CurrentStatement.WhereClauses.Add(@where);
                        break;
                    case OrderByClause orderBy:
                        CurrentStatement.Orderings.AddRange(orderBy.Orderings);
                        break;
                    case AdditionalFromClause additional:
                        var isComplex = queryModel.BodyClauses.Count > i + 1 || queryModel.ResultOperators.Any();
                        var elementType = additional.ItemType;
                        var collectionField = storage.Fields.FieldFor(additional.FromExpression);

                        CurrentStatement = CurrentStatement.ToSelectMany(collectionField, _session, isComplex, elementType);


                        break;
                    case AsJsonResultOperator json:
                        CurrentStatement.ToJsonSelector();
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            if (considerSelectors)
            {
                handleSelector();
            }

            foreach (var resultOperator in queryModel.ResultOperators)
            {
                AddResultOperator(resultOperator);
            }
        }

        public Statement CurrentStatement { get; set; }

        public Statement TopStatement { get; private set; }


        public QueryModel Model { get; }

        private void AddResultOperator(ResultOperatorBase resultOperator)
        {
            switch (resultOperator)
            {
                case TakeResultOperator take:
                    CurrentStatement.Limit = (int)take.Count.Value();
                    break;

                case SkipResultOperator skip:
                    CurrentStatement.Offset = (int)skip.Count.Value();
                    break;

                case AnyResultOperator _:
                    CurrentStatement.ToAny();
                    break;

                case CountResultOperator _:
                    CurrentStatement.ToCount<int>();
                    break;

                case LongCountResultOperator _:
                    CurrentStatement.ToCount<long>();
                    break;

                case FirstResultOperator first:
                    CurrentStatement.Limit = 1;
                    CurrentStatement.SingleValue = true;
                    CurrentStatement.ReturnDefaultWhenEmpty = first.ReturnDefaultWhenEmpty;
                    CurrentStatement.CanBeMultiples = true;
                    break;

                case SingleResultOperator single:
                    CurrentStatement.Limit = 2;
                    CurrentStatement.SingleValue = true;
                    CurrentStatement.ReturnDefaultWhenEmpty = single.ReturnDefaultWhenEmpty;
                    CurrentStatement.CanBeMultiples = false;
                    break;

                case DistinctResultOperator _:
                    CurrentStatement.ApplySqlOperator("distinct");
                    break;

                case AverageResultOperator _:
                    CurrentStatement.ApplyAggregateOperator("AVG");
                    break;

                case SumResultOperator _:
                    CurrentStatement.ApplyAggregateOperator("SUM");
                    break;

                case MinResultOperator _:
                    CurrentStatement.ApplyAggregateOperator("MIN");
                    break;

                case MaxResultOperator _:
                    CurrentStatement.ApplyAggregateOperator("MAX");
                    break;

                case AsJsonResultOperator _:
                    CurrentStatement.ToJsonSelector();
                    break;

                default:
                    throw new NotSupportedException("Don't yet know how to deal with " + resultOperator);
            }
        }

        public IQueryHandler<TResult> BuildHandler<TResult>(QueryStatistics statistics, IList<IInclude> includes)
        {
            if (statistics != null)
            {
                CurrentStatement.UseStatistics(statistics);
            }

            // TODO -- expression parser should be a singleton somehow to avoid
            // the object allocations
            TopStatement.CompileStructure(new MartenExpressionParser(_session.Serializer, _session.Options));

            if (includes.Any())
            {
                TopStatement = new IncludeIdentitySelectorStatement((DocumentStatement) TopStatement, includes);
            }

            var handler = buildHandlerForCurrentStatement<TResult>();

            return includes.Any()
                ? new IncludeQueryHandler<TResult>(handler, includes.Select(x => x.BuildReader(_session)).ToArray())
                : handler;
        }

        private IQueryHandler<TResult> buildHandlerForCurrentStatement<TResult>()
        {
            if (CurrentStatement.SingleValue)
            {
                return CurrentStatement.BuildSingleResultHandler<TResult>(_session, TopStatement);
            }

            return CurrentStatement.SelectClause.BuildHandler<TResult>(_session, TopStatement, CurrentStatement);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryHandler<TResult> BuildHandler<TDocument, TResult>(ISelector<TDocument> selector,
            Statement statement)
        {
            if (typeof(TResult).CanBeCastTo<IEnumerable<TDocument>>())
            {
                return (IQueryHandler<TResult>)new ListQueryHandler<TDocument>(statement, selector);
            }

            throw new NotSupportedException("Marten does not know how to use result type " + typeof(TResult).FullNameInCode());
        }

        public void BuildDiagnosticCommand(FetchType fetchType, CommandBuilder sql)
        {
            switch (fetchType)
            {
                case FetchType.Any:
                    CurrentStatement.ToAny();
                    break;

                case FetchType.Count:
                    CurrentStatement.ToCount<long>();
                    break;

                case FetchType.FetchOne:
                    CurrentStatement.Limit = 1;
                    break;
            }

            // Use a flyweight for MartenExpressionParser
            TopStatement.CompileStructure(new MartenExpressionParser(_session.Serializer, _session.Options));

            TopStatement.Configure(sql);
        }
    }
}
