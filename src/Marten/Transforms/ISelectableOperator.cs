using Marten.Linq;
using Marten.Schema;
using Marten.Storage;
using Marten.V4Internals;
using Marten.V4Internals.Linq;

namespace Marten.Transforms
{
    public interface ISelectableOperator
    {
        Statement ModifyStatement(Statement statement, IMartenSession session);
    }
}
