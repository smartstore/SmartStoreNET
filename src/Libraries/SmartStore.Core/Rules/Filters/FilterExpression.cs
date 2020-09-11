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
    }
}
