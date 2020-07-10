using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Dates;
using LamarCodeGeneration;
using Marten.Linq;
using Marten.Util;
using Marten.V4Internals.Linq;
using Marten.V4Internals.Linq.Includes;
using Npgsql;

namespace Marten.V4Internals.Compiled
{




    public class QueryCompiler
    {
        private static readonly IList<IParameterFinder> _finders = new List<IParameterFinder>();

        private static void forType<T>(Func<int, T[]> uniqueValues)
        {
            var finder = new SimpleParameterFinder<T>(uniqueValues);
            _finders.Add(finder);
        }

        static QueryCompiler()
        {
            forType(count =>
            {
                var values = new string[count];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = Guid.NewGuid().ToString();
                }

                return values;
            });

            forType(count =>
            {
                var values = new Guid[count];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = Guid.NewGuid();
                }

                return values;
            });

            forType(count =>
            {
                var value = -100000;
                var values = new int[count];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = value--;
                }

                return values;
            });

            forType(count =>
            {
                var value = -200000L;
                var values = new long[count];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = value--;
                }

                return values;
            });

            forType(count =>
            {
                var value = -300000L;
                var values = new float[count];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = value--;
                }

                return values;
            });

            forType(count =>
            {
                var value = -300000L;
                var values = new decimal[count];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = value--;
                }

                return values;
            });

            forType(count =>
            {
                var value = new DateTime(1600, 1, 1);
                var values = new DateTime[count];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = value.AddDays(-1);
                }

                return values;
            });

            forType(count =>
            {
                var value = new DateTimeOffset(1600, 1, 1, 0, 0, 0, 0.Seconds());
                var values = new DateTimeOffset[count];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = value.AddDays(-1);
                }

                return values;
            });
        }

        public static CompiledQueryPlan BuildPlan<TDoc, TOut>(IMartenSession session, ICompiledQuery<TDoc, TOut> query)
        {
            eliminateStringNulls(query);

            var plan = new CompiledQueryPlan(query.GetType(), typeof(TOut));
            findIncludes(session, query, plan);

            findQueryStatisticsMember(query, plan);


            assertValidityOfQueryType(plan, query.GetType());

            NpgsqlCommand commandSource(ICompiledQuery<TDoc, TOut> query1)
            {
                Expression expression = query1.QueryIs();
                var invocation = Expression.Invoke(expression, Expression.Parameter(typeof(IMartenQueryable<TDoc>)));

                var builder = new LinqHandlerBuilder(session, invocation, forCompiled:true);

                var command = new NpgsqlCommand();
                var sql = new CommandBuilder(command);
                builder.BuildDiagnosticCommand(FetchType.FetchMany, sql);
                command.CommandText = sql.ToString();

                return command;
            }

            var q = (ICompiledQuery<TDoc, TOut>)Activator.CreateInstance(query.GetType());;
            var singles = new List<IParameterFinder>();
            var multiples = new List<IParameterFinder>();
            foreach (var finder in _finders)
            {
                var match = finder.TryMatch(q);
                switch (match)
                {
                    case SearchMatch.SinglePass:
                        singles.Add(finder);
                        break;

                    case SearchMatch.MultiplePass:
                        multiples.Add(finder);
                        break;
                }
            }


            // Building the inner handler
            Expression expression = query.QueryIs();
            var invocation = Expression.Invoke(expression, Expression.Parameter(typeof(IMartenQueryable<TDoc>)));

            var builder = new LinqHandlerBuilder(session, invocation, forCompiled:true);
            // Use empty include plans because you mostly care about the inner most handler anyway
            var statistics = plan.GetStatisticsIfAny(query);

            plan.HandlerPrototype = builder.BuildHandler<TOut>(statistics, new List<IIncludePlan>());


            if (singles.Any())
            {
                var cmd = commandSource(q);
                plan.Parameters.AddRange(singles.SelectMany(x => x.SinglePassMatch(q, cmd)));
            }

            plan.Parameters.AddRange(multiples.SelectMany(x => x.MultiplePassMatch<TDoc, TOut>(commandSource, query.GetType())));

            // This HAS to be the original query
            plan.Command = commandSource(query);

            return plan;
        }

        private static void eliminateStringNulls(object query)
        {
            var type = query.GetType();

            foreach (var propertyInfo in type.GetProperties().Where(x => x.CanWrite && x.PropertyType == typeof(string)))
            {
                var raw = propertyInfo.GetValue(query);
                if (raw == null)
                {
                    propertyInfo.SetValue(query, string.Empty);
                }
            }

            foreach (var fieldInfo in type.GetFields().Where(x => x.FieldType == typeof(string)))
            {
                var raw = fieldInfo.GetValue(query);
                if (raw == null)
                {
                    fieldInfo.SetValue(query, string.Empty);
                }
            }
        }

        private static void findQueryStatisticsMember<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query, CompiledQueryPlan plan)
        {
            plan.StatisticsMember = query.GetType().GetProperties()
                .FirstOrDefault(x => x.PropertyType == typeof(QueryStatistics));
        }

        private static void findIncludes<TDoc, TOut>(IMartenSession session, ICompiledQuery<TDoc, TOut> query, CompiledQueryPlan plan)
        {
            var expression = query.QueryIs();
            var invocation = Expression.Invoke(expression, Expression.Parameter(typeof(IMartenQueryable<TDoc>)));

            var includeVisitor = new CompiledQueryExpressionVisitor<TDoc>(session, plan);
            includeVisitor.Visit(invocation.Expression);
        }

        private static void assertValidityOfQueryType(ICompiledQueryPlan plan, Type type)
        {
            try
            {
                Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Marten requires a no-argument constructor for compiled query types. This constructor does not have to be public");
            }

            var specialMembers = plan.SpecialMembers();

            foreach (var field in type.GetFields(BindingFlags.Instance).Where(x => !specialMembers.Contains(x)))
            {
                if (_finders.All(x => x.DotNetType != field.FieldType))
                {
                    throw new InvalidOperationException($"Marten does not (yet) support fields of type {field.FieldType.FullNameInCode()} as arguments to compiled queries");
                }
            }

            foreach (var property in type.GetProperties(BindingFlags.Instance).Where(x => !specialMembers.Contains(x)))
            {
                if (_finders.All(x => x.DotNetType != property.PropertyType))
                {
                    throw new InvalidOperationException($"Marten does not (yet) support properties of type {property.PropertyType.FullNameInCode()} as arguments to compiled queries");
                }

                if (!property.CanWrite)
                {
                    throw new InvalidOperationException($"Property {property.Name} must have a setter for compiled query planning");
                }
            }
        }
    }
}
