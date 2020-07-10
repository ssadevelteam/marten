using System;
using System.Reflection;
using LamarCodeGeneration;
using Marten.Util;

namespace Marten.V4Internals.Compiled
{
    public class ParameterMap
    {
        public ParameterMap(int parameterIndex, MemberInfo member)
        {
            ParameterIndex = parameterIndex;
            Member = member;
        }

        public void GenerateCode(GeneratedMethod method)
        {
            method.Frames.Code($@"
parameters[{ParameterIndex}].NpgsqlDbType = {{0}};
parameters[{ParameterIndex}].Value = _query.{Member.Name};
", TypeMappings.ToDbType(Member.GetMemberType()));
        }

        public int ParameterIndex { get; }
        public MemberInfo Member { get; }
    }


    /* TODO
     By Type:
DateTime
DateTimeOffset
int
long
bool -- special
string








     */
}
