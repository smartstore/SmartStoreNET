using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Customers;
using SmartStore.Rules;
using SmartStore.Rules.Filters;

namespace SmartStore.Services.Customers
{
    public class TargetGroupFilterDescriptor : FilterDescriptor<Customer, bool>
    {
        private readonly IRuleFactory _ruleFactory;
        private readonly WeakReference<IRuleVisitor> _ruleVisitor;

        public TargetGroupFilterDescriptor(IRuleFactory ruleFactory, IRuleVisitor ruleVisitor)
            : base(x => true)
        {
            _ruleFactory = ruleFactory;
            _ruleVisitor = new WeakReference<IRuleVisitor>(ruleVisitor);
        }

        public override Expression GetExpression(RuleOperator op, Expression valueExpression, bool liftToNull)
        {
            var ruleSetId = ((ConstantExpression)valueExpression).Value.Convert<int>();

            // Get other expression group
            _ruleVisitor.TryGetTarget(out var visitor);
            var otherGroup = _ruleFactory.CreateExpressionGroup(ruleSetId, visitor) as FilterExpressionGroup;

            var otherPredicate = otherGroup?.ToPredicate(liftToNull);
            if (otherPredicate is Expression<Func<Customer, bool>> lambda)
            {
                MemberExpression = lambda;
            }

            return base.GetExpression(op, Expression.Constant(true), liftToNull);
        }
    }
}
