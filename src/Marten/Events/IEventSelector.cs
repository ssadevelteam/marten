using Marten.Util;
using Marten.V4Internals;

namespace Marten.Events
{
    internal interface IEventSelector: ISelector<IEvent>
    {
        EventGraph Events { get; }
        void WriteSelectClause(CommandBuilder sql);
        string[] SelectFields();
    }
}
