using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Marten.Util;
using Marten.V4Internals;

namespace Marten
{
    internal class SingleDocumentJsonLoader<T>: IQueryHandler<string>
    {
        private readonly IDocumentStorage<T> _storage;
        private readonly object _id;

        public SingleDocumentJsonLoader(IDocumentStorage<T> storage, object id)
        {
            _storage = storage;
            _id = id;
        }

        public void ConfigureCommand(CommandBuilder builder, IMartenSession session)
        {
            builder.Append("select data from ");
            builder.Append(_storage.FromObject);
            builder.Append(" where id = :");

            var parameter = builder.AddParameter(_id);
            builder.Append(parameter.ParameterName);
        }

        public string Handle(DbDataReader reader, IMartenSession session)
        {
            return reader.Read()
                ? reader.GetFieldValue<string>(0)
                : null;
        }

        public async Task<string> HandleAsync(DbDataReader reader, IMartenSession session, CancellationToken token)
        {
            if (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                return await reader.GetFieldValueAsync<string>(0, token).ConfigureAwait(false);
            }

            return null;
        }
    }
}
