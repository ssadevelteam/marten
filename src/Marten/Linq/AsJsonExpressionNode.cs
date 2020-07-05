using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Marten.Linq
{
    public class AsJsonMatcher: IMethodCallMatcher
    {
        public bool TryMatch(MethodCallExpression expression, out ResultOperatorBase op)
        {
            if (AsJsonExpressionNode.SupportedMethods.Contains(expression.Method))
            {
                op = AsJsonResultOperator.Flyweight;
                return true;
            }

            op = null;
            return false;
        }
    }

    public class AsJsonExpressionNode: ResultOperatorExpressionNodeBase
    {
        public static MethodInfo[] SupportedMethods =
            typeof(CompiledQueryExtensions).GetMethods().Where(m => m.Name == nameof(CompiledQueryExtensions.AsJson)).ToArray();

        public AsJsonExpressionNode(
            MethodCallExpressionParseInfo parseInfo)
            : base(parseInfo, null, null) { }

        protected override ResultOperatorBase CreateResultOperator(
            ClauseGenerationContext clauseGenerationContext)
        {
            return AsJsonResultOperator.Flyweight;
        }

        public override Expression Resolve(
            ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(
                inputParameter,
                expressionToBeResolved,
                clauseGenerationContext);
        }


    }
}
