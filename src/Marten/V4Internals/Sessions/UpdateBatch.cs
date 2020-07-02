using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten.Schema.Arguments;
using Marten.Services;
using Marten.Util;
using Npgsql;

namespace Marten.V4Internals.Sessions
{
    public class UpdateBatch
    {
        private readonly IReadOnlyList<IStorageOperation> _operations;

        private readonly IList<Exception> _exceptions = new List<Exception>();

        public UpdateBatch(IReadOnlyList<IStorageOperation> operations)
        {
            _operations = operations;
        }

        public void ApplyChanges(IMartenSession session)
        {
            if (_operations.Count < session.Options.UpdateBatchSize)
            {
                var command = buildCommand(session, _operations);
                var reader = session.Database.ExecuteReader(command);
                applyCallbacks(_operations, reader);
            }
            else
            {
                throw new NotImplementedException("Not yet doing BIG batch sizes");
            }

            if (_exceptions.Any()) throw new AggregateException(_exceptions);
        }

        public async Task ApplyChangesAsync(IMartenSession session, CancellationToken token)
        {
            if (_operations.Count < session.Options.UpdateBatchSize)
            {
                var command = buildCommand(session, _operations);
                var reader = await session.Database.ExecuteReaderAsync(command, token).ConfigureAwait(false);
                await applyCallbacksAsync(_operations, reader, token).ConfigureAwait(false);
            }
            else
            {
                throw new NotImplementedException("Not yet doing BIG batch sizes");
            }

            if (_exceptions.Any()) throw new AggregateException(_exceptions);
        }

        private void applyCallbacks(IEnumerable<IStorageOperation> operations, DbDataReader reader)
        {
            var first = operations.First();
            first.Postprocess(reader, _exceptions);

            foreach (var operation in operations.Skip(1))
            {
                if (!(operation is NoDataReturnedCall))
                {
                    reader.NextResult();
                    operation.Postprocess(reader, _exceptions);
                }
            }
        }

        private async Task applyCallbacksAsync(IEnumerable<IStorageOperation> operations, DbDataReader reader, CancellationToken token)
        {
            var first = operations.First();
            await first.PostprocessAsync(reader, _exceptions, token).ConfigureAwait(false);

            foreach (var operation in operations.Skip(1))
            {
                if (!(operation is NoDataReturnedCall))
                {
                    await reader.NextResultAsync(token).ConfigureAwait(false);
                    await operation.PostprocessAsync(reader, _exceptions, token).ConfigureAwait(false);
                }
            }
        }

        private NpgsqlCommand buildCommand(IMartenSession session, IEnumerable<IStorageOperation> operations)
        {
            var command = new NpgsqlCommand();
            var builder = new CommandBuilder(command);
            foreach (var operation in operations)
            {
                operation.ConfigureCommand(builder, session);
            }

            // Duplication here!
            command.CommandText = builder.ToString();

            // TODO -- Like this to be temporary
            if (command.CommandText.Contains(CommandBuilder.TenantIdArg))
            {
                command.AddNamedParameter(TenantIdArgument.ArgName, session.Tenant.TenantId);
            }

            return command;
        }


    }
}
