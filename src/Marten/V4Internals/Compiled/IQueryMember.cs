using System;
using System.Reflection;
using LamarCodeGeneration;
using Npgsql;

namespace Marten.V4Internals.Compiled
{
    public interface IQueryMember
    {
        Type Type { get; }
        bool CanWrite();

        MemberInfo Member { get; }
        int ParameterIndex { get; set; }
        void GenerateCode(GeneratedMethod method);
        void StoreValue(object query);
        void TryMatch(NpgsqlCommand command);
        void TryWriteValue(UniqueValueSource valueSource, object query);
    }
}
