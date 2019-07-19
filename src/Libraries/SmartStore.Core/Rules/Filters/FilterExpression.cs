using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace SmartStore.Rules.Filters
{
    public class FilterExpression : RuleExpression
    {
        public new FilterDescriptor Descriptor { get; set; }

        public virtual Expression ToPredicate(ParameterExpression node, bool liftToNull)
        {
            return CreateBodyExpression(node, liftToNull);
        }

        protected virtual Expression CreateBodyExpression(ParameterExpression node, bool liftToNull)
        {
            return this.Descriptor.GetExpression(
                this.Operator,
                ExpressionHelper.CreateValueExpression(Descriptor.MemberExpression.Body.Type, this.Value), 
                liftToNull);
        }

        //public Expression CreateMemberExpression()
        //{
        //    Type memberType = _filter.Descriptor.Type.ClrType;
        //    var memberName = _filter.Descriptor.Member;

        //    var expression = CreateMemberAccessExpression(_parameterExpression, memberType, memberName);
        //    if ((memberType != null) && (expression.Type.GetNonNullableType() != memberType.GetNonNullableType()))
        //    {
        //        expression = Expression.Convert(expression, memberType);
        //    }

        //    return expression;
        //}

        //private Expression CreateMemberAccessExpression(Expression instance, Type memberType, string memberName)
        //{
        //    foreach (var token in MemberAccessTokenizer.GetTokens(memberName))
        //    {
        //        instance = token.CreateMemberAccessExpression(_parameterExpression);
        //    }

        //    return instance;
        //}
    }
}
