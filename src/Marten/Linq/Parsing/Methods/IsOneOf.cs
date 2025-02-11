using System.Linq;
using System.Linq.Expressions;
using Marten.Linq.Fields;
using Marten.Linq.Filters;
using Marten.Linq.SqlGeneration;
using Marten.Util;
using Weasel.Postgresql.SqlGeneration;

namespace Marten.Linq.Parsing.Methods
{
    internal class IsOneOf: IMethodCallParser
    {
        public bool Matches(MethodCallExpression expression)
        {
            return (expression.Method.Name == nameof(LinqExtensions.IsOneOf)
                    || expression.Method.Name == nameof(LinqExtensions.In))
                   && expression.Method.DeclaringType == typeof(LinqExtensions);
        }

        public ISqlFragment Parse(IFieldMapping mapping, ISerializer serializer, MethodCallExpression expression)
        {
            var members = FindMembers.Determine(expression);

            var locator = mapping.FieldFor(members).TypedLocator;
            var values = expression.Arguments.Last().Value();

            if (members.Last().GetMemberType().IsEnum)
            {
                return new EnumIsOneOfWhereFragment(values, serializer.EnumStorage, locator);
            }

            // TODO -- use new WhereInArray
            return new WhereFragment($"{locator} = ANY(?)", values);
        }
    }
}
