using System.Linq.Expressions;
using Remotion.Linq.Clauses;

namespace Marten.Linq
{
    public interface IMethodCallMatcher
    {
        bool TryMatch(MethodCallExpression expression, out ResultOperatorBase op);
    }
}
