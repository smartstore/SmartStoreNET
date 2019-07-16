using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Filters
{
    public class CompositeFilterExpression : FilterExpression
    {
        private readonly List<FilterExpression> _expressions = new List<FilterExpression>();

        public LogicalRuleOperator LogicalOperator { get; set; }
        public IReadOnlyCollection<FilterExpression> Expressions
        {
            get => _expressions;
        }

        public void AddExpression(FilterExpression expression)
        {
            Guard.NotNull(expression, nameof(expression));
            _expressions.Add(expression);
        }

        public override Expression CreateLambdaExpression(Expression instance)
        {
            Expression left = null;

            foreach (var ruleExpression in Expressions)
            {
                var parameterExpression = Expression.Parameter(base.Descriptor.EntityType, "x");
                var right = ruleExpression.CreateLambdaExpression(parameterExpression);

                if (left == null)
                    left = right;
                else
                    left = CombineExpressions(left, right, LogicalOperator);
            }

            if (left == null)
            {
                return ExpressionHelper.TrueLiteral;
            }

            return left;
        }

        private Expression CombineExpressions(Expression left, Expression right, LogicalRuleOperator logicalOperator)
        {
            return logicalOperator == LogicalRuleOperator.And
                ? Expression.AndAlso(left, right)
                : Expression.OrElse(left, right);
        }



    }
}
