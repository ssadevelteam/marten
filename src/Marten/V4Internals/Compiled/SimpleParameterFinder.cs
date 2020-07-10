using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Marten.Linq;
using Npgsql;

namespace Marten.V4Internals.Compiled
{
    public class SimpleParameterFinder<T> : IParameterFinder
    {
        private readonly Func<int, T[]> _uniqueValues;

        public SimpleParameterFinder(Func<int, T[]> uniqueValues)
        {
            _uniqueValues = uniqueValues;
        }

        public Type DotNetType => typeof(T);


        public SearchMatch TryMatch(object query)
        {
            var members = findMembers(query.GetType()).ToArray();
            if (!members.Any())
            {
                return SearchMatch.None;
            }

            var values = _uniqueValues(members.Length);
            for (var i = 0; i < values.Length; i++)
            {
                if (members[i] is PropertyInfo p) p.SetValue(query, values[i]);

                if (members[i] is FieldInfo f) f.SetValue(query, values[i]);
            }

            return SearchMatch.SinglePass;
        }

        private IEnumerable<MemberInfo> findMembers(Type type)
        {
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.CanRead && x.PropertyType == typeof(T)))
            {
                yield return prop;
            }

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public).Where(x => x.FieldType == typeof(T)))
            {
                yield return field;
            }
        }

        private IEnumerable<(MemberInfo member, object value)> findMemberValues(object query)
        {
            foreach (var prop in query.GetType().GetProperties().Where(x => x.CanRead && x.PropertyType == typeof(T)))
            {
                yield return (prop, prop.GetValue(query));
            }

            foreach (var field in query.GetType().GetFields().Where(x => x.FieldType == typeof(T)))
            {
                yield return (field, field.GetValue(query));
            }
        }

        public IEnumerable<ParameterMap> SinglePassMatch(object query, NpgsqlCommand command)
        {
            var members = findMemberValues(query).ToArray();

            for (int i = 0; i < command.Parameters.Count; i++)
            {
                var parameter = command.Parameters[i];
                var match = members.SingleOrDefault(x => x.value.Equals(parameter.Value));

                if (match != default)
                {
                    yield return new ParameterMap(i, match.member);
                }

            }
        }

        public IEnumerable<ParameterMap> MultiplePassMatch<TDoc, TOut>(Func<ICompiledQuery<TDoc, TOut>, NpgsqlCommand> source, Type queryType)
        {
            throw new NotSupportedException();
        }
    }
}
