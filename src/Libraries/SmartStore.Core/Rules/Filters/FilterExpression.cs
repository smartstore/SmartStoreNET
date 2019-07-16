using System;
using System.Linq.Expressions;

namespace SmartStore.Rules.Filters
{
    public class FilterExpression : RuleExpression
    {
        public new FilterDescriptor Descriptor { get; set; }

        public virtual Expression CreateLambdaExpression(Expression instance)
        {
            if (instance is ParameterExpression parameterExpression)
            {
                var builder = new FilterExpressionBuilder(parameterExpression, this);
                return builder.CreateBodyExpression();
            }

            throw new ArgumentException("Parameter should be of type 'ParameterExpression'.", nameof(instance));
        }
    }
}
