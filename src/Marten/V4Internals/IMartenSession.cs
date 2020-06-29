using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Marten.Services;
using Marten.Storage;
using Marten.Util;
using Npgsql;
using Remotion.Linq.Clauses;

namespace Marten.V4Internals
{
    public interface IMartenSession : IDisposable
    {
        ISerializer Serializer { get; }
        Dictionary<Type, object> ItemMap { get; }
        ITenant Tenant { get; }

        VersionTracker Versions { get; }

        IManagedConnection Database { get; }
        IDocumentStorage StorageFor(Type documentType);

        StoreOptions Options { get; }

    }

    internal static class MartenSessionExtensions
    {

    }
}
