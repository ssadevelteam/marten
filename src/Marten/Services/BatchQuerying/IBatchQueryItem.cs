using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Marten.Linq;
using Marten.Linq.QueryHandlers;
using Marten.V4Internals;

namespace Marten.Services.BatchQuerying
{
    public interface IBatchQueryItem
    {
        IQueryHandler Handler { get; }

        QueryStatistics Stats { get; }

        Task ReadAsync(DbDataReader reader, IMartenSession session, CancellationToken token);

        void Read(DbDataReader reader, IMartenSession session);
    }
}
