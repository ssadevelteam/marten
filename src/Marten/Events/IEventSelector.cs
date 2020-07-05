using Marten.Linq;
using Marten.V4Internals;

namespace Marten.Events
{
    // TODO -- this needs to implement ISelectClause
    internal interface IEventSelector: ISelector<IEvent>
    {
        EventGraph Events { get; }
    }
}
