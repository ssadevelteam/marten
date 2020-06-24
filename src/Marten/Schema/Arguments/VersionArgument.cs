using System;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using Marten.Schema.Identity;
using NpgsqlTypes;

namespace Marten.Schema.Arguments
{
    public class VersionArgument: UpsertArgument
    {
        public const string ArgName = "docVersion";

        private readonly static MethodInfo _newGuid =
            typeof(Guid).GetMethod(nameof(Guid.NewGuid),
                BindingFlags.Static | BindingFlags.Public);

        public VersionArgument()
        {
            Arg = ArgName;
            Column = DocumentMapping.VersionColumn;
            DbType = NpgsqlDbType.Uuid;
            PostgresType = "uuid";
        }


        public override Expression CompileUpdateExpression(EnumStorage enumStorage, ParameterExpression call, ParameterExpression doc, ParameterExpression updateBatch, ParameterExpression mapping, ParameterExpression currentVersion, ParameterExpression newVersion, ParameterExpression tenantId, bool useCharBufferPooling)
        {
            var dbType = Expression.Constant(DbType);
            return Expression.Call(call, _paramMethod, Expression.Constant(Arg), Expression.Convert(newVersion, typeof(object)), dbType);
        }

        public override void GenerateCode(GeneratedMethod method, GeneratedType type, int i, Argument parameters,
            DocumentMapping mapping)
        {
            method.Frames.Code("setVersionParameter({0}[{1}]);", parameters, i);
        }

        public override void GenerateBulkWriterCode(GeneratedType type, GeneratedMethod load, DocumentMapping mapping)
        {
            if (mapping.VersionMember == null)
            {
                load.Frames.Code($"writer.Write({typeof(CombGuidIdGeneration).FullNameInCode()}.NewGuid(), {{0}});", NpgsqlDbType.Uuid);
            }
            else
            {
                load.Frames.Code($@"
var version = {typeof(CombGuidIdGeneration).FullNameInCode()}.NewGuid();
document.{mapping.VersionMember.Name} = version;
writer.Write(version, {{0}});
", NpgsqlDbType.Uuid);
            }


        }
    }
}
