using System;
using System.Linq.Expressions;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using NpgsqlTypes;

namespace Marten.Schema.Arguments
{
    public class CurrentVersionArgument: UpsertArgument
    {
        public CurrentVersionArgument()
        {
            Arg = "current_version";
            PostgresType = "uuid";
            DbType = NpgsqlDbType.Uuid;
            Column = null;
        }

        public override Expression CompileUpdateExpression(EnumStorage enumStorage, ParameterExpression call, ParameterExpression doc, ParameterExpression updateBatch, ParameterExpression mapping, ParameterExpression currentVersion, ParameterExpression newVersion, ParameterExpression tenantId, bool useCharBufferPooling)
        {
            var argName = Expression.Constant(Arg);

            return Expression.Call(call, _paramMethod, argName, Expression.Convert(currentVersion, typeof(object)), Expression.Constant(DbType));
        }

        public override void GenerateCode(GeneratedMethod method, GeneratedType type, int i, Argument parameters,
            DocumentMapping mapping)
        {
            method.Frames.Code("setCurrentVersionParameter({0}[{1}]);", parameters, i);
        }
    }
}
