using System.Linq.Expressions;

namespace SmartStore.Rules.Filters
{
    internal sealed class FilterExpressionVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _rootParameter;

        public FilterExpressionVisitor(ParameterExpression rootParameter)
        {
            _rootParameter = rootParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // The parameter expressions in each filter (x => x.ProductId) must point to the root parameter (it => ...),
            // that was created by CompositeFilterExpression.
            if (node.Type == _rootParameter.Type)
            {
                return _rootParameter;
            }

            // When target types do not match, then the parameter most likely serves a deeper opject path.
            return base.VisitParameter(node);
        }
    }
}
