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

        public CompositeFilterExpression(Type entityType)
        {
            Guard.NotNull(entityType, nameof(entityType));

            EntityType = entityType;
            LogicalOperator = LogicalRuleOperator.And;
            Operator = RuleOperator.IsEqualTo;
            Value = true;
        }

        public Type EntityType { get; private set; }
        public LogicalRuleOperator LogicalOperator { get; set; }
        public IReadOnlyCollection<FilterExpression> Expressions
        {
            get => _expressions;
        }

        public void AddExpressions(params FilterExpression[] expressions)
        {
            Guard.NotNull(expressions, nameof(expressions));
            _expressions.AddRange(expressions);
        }

        public Expression ToPredicate(bool liftToNull)
        {
            return ToPredicate(null, liftToNull);
        }

        public override Expression ToPredicate(ParameterExpression node, bool liftToNull)
        {
            if (node == null)
            {
                //instance = Expression.Parameter(base.Descriptor.MemberExpression.Type, "it"); // TODO: was base.Descriptor.EntityType, check if MemberExpression is the same
                node = Expression.Parameter(EntityType, "it"); // TODO: was base.Descriptor.EntityType, check if MemberExpression is the same
            }

            return ExpressionHelper.CreateLambdaExpression(node, base.ToPredicate(node, liftToNull));
        }

        protected override Expression CreateBodyExpression(ParameterExpression node, bool liftToNull)
        {
            Expression left = null;

            foreach (var ruleExpression in Expressions)
            {
                var right = ruleExpression.ToPredicate(node, liftToNull);

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
