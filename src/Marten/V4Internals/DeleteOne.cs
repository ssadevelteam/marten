using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Marten.Services;
using Marten.Util;

namespace Marten.V4Internals
{


    public abstract class DeleteOne<T, TId>: IDeletion
    {
        public DeleteOne(TId id)
        {
            Id = id;
        }

        public abstract void ConfigureCommand(CommandBuilder builder, IMartenSession session);

        public Type DocumentType => typeof(T);

        public TId Id { get; }

        public void Postprocess(DbDataReader reader, IList<Exception> exceptions)
        {
            // Nothing yet
        }

        public Task PostprocessAsync(DbDataReader reader, IList<Exception> exceptions, CancellationToken token)
        {
            // Nothing yet
            return Task.CompletedTask;
        }

        public StorageRole Role()
        {
            return StorageRole.Deletion;
        }

    }
}
