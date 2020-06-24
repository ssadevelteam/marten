using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using Marten.Services;
using Marten.Util;
using Npgsql;
using NpgsqlTypes;

namespace Marten.Schema.Arguments
{
    public class UpsertArgument
    {
        protected static readonly MethodInfo writeMethod =
            typeof(NpgsqlBinaryImporter).GetMethods().FirstOrDefault(x => x.Name == "Write" && x.GetParameters().Length == 2 && x.GetParameters()[0].ParameterType.IsGenericParameter && x.GetParameters()[1].ParameterType == typeof(NpgsqlTypes.NpgsqlDbType));

        protected static readonly MethodInfo _paramMethod = typeof(SprocCall)
            .GetMethod("Param", new[] { typeof(string), typeof(object), typeof(NpgsqlDbType) });

        protected static readonly MethodInfo _paramWithJsonBody = typeof(SprocCall)
            .GetMethod("JsonBody", new[] { typeof(string), typeof(ArraySegment<char>) });

        private MemberInfo[] _members;
        private string _postgresType;
        public string Arg { get; set; }

        public string PostgresType
        {
            get => _postgresType;
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                _postgresType = value.Contains("(")
                    ? value.Split('(')[0].Trim()
                    : value;
            }
        }

        public string Column { get; set; }

        public MemberInfo[] Members
        {
            get => _members;
            set
            {
                _members = value;
                if (value != null)
                {
                    DbType = TypeMappings.ToDbType(value.Last().GetMemberType());
                }
            }
        }

        public Type DotNetType => _members?.LastOrDefault()?.GetMemberType();

        public NpgsqlDbType DbType { get; set; }

        public string ArgumentDeclaration()
        {
            return $"{Arg} {PostgresType}";
        }

        [Obsolete("Will go away in v4")]
        public virtual Expression CompileUpdateExpression(EnumStorage enumStorage, ParameterExpression call, ParameterExpression doc, ParameterExpression updateBatch, ParameterExpression mapping, ParameterExpression currentVersion, ParameterExpression newVersion, ParameterExpression tenantId, bool useCharBufferPooling)
        {
            var argName = Expression.Constant(Arg);

            var memberType = Members.Last().GetMemberType();
            var body = LambdaBuilder.ToExpression(enumStorage, Members, doc);
            if (!memberType.GetTypeInfo().IsClass)
            {
                body = Expression.Convert(body, typeof(object));
            }

            return Expression.Call(call, _paramMethod, argName, body, Expression.Constant(DbType));
        }

        public virtual void GenerateCode(GeneratedMethod method, GeneratedType type, int i, Argument parameters,
            DocumentMapping mapping)
        {
            if (DotNetType.IsEnum)
            {
                if (mapping.EnumStorage == EnumStorage.AsInteger)
                {
                    method.Frames.Code($"{parameters.Usage}[{i}].{nameof(NpgsqlParameter.NpgsqlDbType)} = {{0}};", NpgsqlDbType.Integer);
                    method.Frames.Code($"{parameters.Usage}[{i}].{nameof(NpgsqlParameter.Value)} = (int)document.{_members.Last().Name};");
                }
                else
                {
                    method.Frames.Code($"{parameters.Usage}[{i}].{nameof(NpgsqlParameter.NpgsqlDbType)} = {{0}};", NpgsqlDbType.Varchar);
                    method.Frames.Code($"{parameters.Usage}[{i}].{nameof(NpgsqlParameter.Value)} = document.{_members.Last().Name}?.ToString();");
                }
            }
            else
            {
                method.Frames.Code($"{parameters.Usage}[{i}].{nameof(NpgsqlParameter.NpgsqlDbType)} = {{0}};", DbType);
                method.Frames.Code($"{parameters.Usage}[{i}].{nameof(NpgsqlParameter.Value)} = document.{_members.Last().Name};");
            }
        }

        public virtual void GenerateBulkWriterCode(GeneratedType type, GeneratedMethod load, DocumentMapping mapping)
        {
            if (DotNetType.IsEnum)
            {
                if (mapping.EnumStorage == EnumStorage.AsInteger)
                {
                    load.Frames.Code($"writer.Write((int)document.{_members.Last().Name}, {{0}});", NpgsqlDbType.Integer);
                }
                else
                {
                    load.Frames.Code($"writer.Write(document.{_members.Last().Name}?.ToString(), {{0}});", NpgsqlDbType.Varchar);
                }
            }
            else
            {
                load.Frames.Code($"writer.Write(document.{_members.Last().Name}, {{0}});", DbType);
            }

        }
    }
}
