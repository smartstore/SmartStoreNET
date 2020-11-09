using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SmartStore.Rules.Filters
{
    public class FilterExpressionGroup : FilterExpression, IRuleExpressionGroup
    {
        private readonly List<IRuleExpression> _expressions = new List<IRuleExpression>();

        public FilterExpressionGroup(Type entityType)
        {
            Guard.NotNull(entityType, nameof(entityType));

            EntityType = entityType;
            LogicalOperator = LogicalRuleOperator.And;
            Operator = RuleOperator.IsEqualTo;
            Value = true;
        }

        public int RefRuleId { get; set; }
        public Type EntityType { get; private set; }
        public LogicalRuleOperator LogicalOperator { get; set; }
        public bool IsSubGroup { get; set; }
        public IRuleProvider Provider { get; set; }

        public IEnumerable<IRuleExpression> Expressions => _expressions;

        public void AddExpressions(params IRuleExpression[] expressions)
        {
            Guard.NotNull(expressions, nameof(expressions));
            _expressions.AddRange(expressions.OfType<FilterExpression>());
        }

        public Expression ToPredicate(bool liftToNull)
        {
            return ToPredicate(null, liftToNull);
        }

        public override Expression ToPredicate(ParameterExpression node, bool liftToNull)
        {
            if (node == null)
            {
                node = Expression.Parameter(EntityType, "it"); // TODO: was base.Descriptor.EntityType, check if MemberExpression is the same
            }

            //return ExpressionHelper.CreateLambdaExpression(node, base.ToPredicate(node, liftToNull));
            return ExpressionHelper.CreateLambdaExpression(node, CreateBodyExpression(node, liftToNull));
        }

        protected override Expression CreateBodyExpression(ParameterExpression node, bool liftToNull)
        {
            Expression left = null;

            foreach (var ruleExpression in Expressions.Cast<FilterExpression>())
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
