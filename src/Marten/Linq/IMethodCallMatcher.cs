using System.Linq.Expressions;
using Marten.V4Internals.Linq;
using Remotion.Linq.Clauses;

namespace Marten.Linq
{
    public interface IMethodCallMatcher
    {
        bool TryMatch(MethodCallExpression expression, ExpressionVisitor selectorVisitor,
            out ResultOperatorBase op);
    }
}
