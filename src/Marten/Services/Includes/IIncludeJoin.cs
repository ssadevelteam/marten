using System;
using Marten.Linq;
using Marten.Schema;
using Marten.Storage;
using Marten.Util;

namespace Marten.Services.Includes
{
    [Obsolete("This abomination is going away in v4")]
    public interface IIncludeJoin
    {
        string JoinText { get; }
        string TableAlias { get; }

        ISelector<TSearched> WrapSelector<TSearched>(StorageFeatures storage, ISelector<TSearched> inner);

        void AppendJoin(CommandBuilder sql, string rootTableAlias, IQueryableDocument document);
    }
}
