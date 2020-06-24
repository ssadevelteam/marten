using System.Linq.Expressions;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Marten.Storage;
using Marten.V4Internals;
using NpgsqlTypes;

namespace Marten.Schema.Arguments
{
    public class TenantIdArgument: UpsertArgument
    {
        public const string ArgName = "tenantid";

        public TenantIdArgument()
        {
            Arg = ArgName;
            PostgresType = "varchar";
            DbType = NpgsqlDbType.Varchar;
            Column = TenantIdColumn.Name;
        }


        public override Expression CompileUpdateExpression(EnumStorage enumStorage, ParameterExpression call, ParameterExpression doc,
            ParameterExpression updateBatch, ParameterExpression mapping, ParameterExpression currentVersion,
            ParameterExpression newVersion, ParameterExpression tenantId, bool useCharBufferPooling)
        {
            var argName = Expression.Constant(Arg);
            var dbType = Expression.Constant(NpgsqlDbType.Varchar);

            return Expression.Call(call, _paramMethod, argName, tenantId, dbType);
        }

        public override void GenerateCode(GeneratedMethod method, GeneratedType type, int i, Argument parameters,
            DocumentMapping mapping)
        {
            method.Frames.Code($"{{0}}[{{1}}].Value = {{2}}.{nameof(IMartenSession.Tenant)}.{nameof(ITenant.TenantId)};", parameters, i, Use.Type<IMartenSession>());
            method.Frames.Code("{0}[{1}].NpgsqlDbType = {2};", parameters, i, DbType);
        }

        public override void GenerateBulkWriterCode(GeneratedType type, GeneratedMethod load, DocumentMapping mapping)
        {
            load.Frames.Code($"writer.Write(tenant.TenantId, {{0}});", DbType);
        }
    }
}
