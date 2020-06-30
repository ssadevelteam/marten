using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Marten.Util;

namespace Marten.V4Internals
{
    public interface IQueryHandler
    {
        void ConfigureCommand(CommandBuilder builder, IMartenSession session);
    }


    public interface IQueryHandler<T>: IQueryHandler
    {
        T Handle(DbDataReader reader, IMartenSession session);

        Task<T> HandleAsync(DbDataReader reader, IMartenSession session, CancellationToken token);
    }
}
