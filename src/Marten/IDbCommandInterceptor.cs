using System.Threading.Tasks;
using Npgsql;

namespace Marten
{
    public interface IDbCommandInterceptor
    {
        void NonQueryExecuting(NpgsqlCommand command);
        void ReaderExecuting(NpgsqlCommand command);
        Task ReaderExecutingAsync(NpgsqlCommand command);
        Task NonQueryExecutingAsync(NpgsqlCommand command);
    }
}
