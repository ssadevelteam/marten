using System;
using System.Linq;
using System.Reflection;
using Baseline;
using LamarCodeGeneration;
using Marten.Util;
using Npgsql;

namespace Marten.V4Internals.Compiled
{
    public interface IQueryMember<T>: IQueryMember
    {
        T GetValue(object query);
        void SetValue(object query, T value);
        T Value { get; }
    }

    public abstract class QueryMember<T> : IQueryMember<T>
    {
        protected QueryMember(MemberInfo member)
        {
            Member = member;
        }

        public Type Type => typeof(T);

        public abstract T GetValue(object query);
        public abstract void SetValue(object query, T value);

        public void StoreValue(object query)
        {
            Value = GetValue(query);
        }

        public void TryMatch(NpgsqlCommand command)
        {
            ParameterIndex = -1;

            if (!isFound(command, Value) && Type == typeof(string))
            {
                if (isFound(command, $"%{Value}"))
                {
                    Mask = "StartsWith({0})";
                }
                else if (isFound(command, $"%{Value}%"))
                {
                    Mask = "ContainsString({0})";
                }
                else if (isFound(command, $"{Value}%"))
                {
                    Mask = "EndsWith({0})";
                }
            }


        }

        private bool isFound(NpgsqlCommand command, object value)
        {
            var parameter = command.Parameters.FirstOrDefault(x => value.Equals(x.Value));
            if (parameter != null)
            {
                ParameterIndex = command.Parameters.IndexOf(parameter);
                return true;
            }

            return false;
        }

        public string Mask { get; set; }

        public void TryWriteValue(UniqueValueSource valueSource, object query)
        {
            if (CanWrite())
            {
                SetValue(query, (T)valueSource.GetValue(typeof(T)));
            }
        }

        public T Value { get; private set; }

        public abstract bool CanWrite();

        public MemberInfo Member { get; }

        public int ParameterIndex { get; set; }

        public void GenerateCode(GeneratedMethod method)
        {
            if (Mask == null)
            {
                method.Frames.Code($@"
parameters[{ParameterIndex}].NpgsqlDbType = {{0}};
parameters[{ParameterIndex}].Value = _query.{Member.Name};
", TypeMappings.ToDbType(Member.GetMemberType()));
            }
            else
            {
                var maskedValue = Mask.ToFormat($"_query.{Member.Name}");

                method.Frames.Code($@"
parameters[{ParameterIndex}].NpgsqlDbType = {{0}};
parameters[{ParameterIndex}].Value = {maskedValue};
", TypeMappings.ToDbType(Member.GetMemberType()));
            }


        }
    }
}
