using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SmartStore.Linq.Expressions
{
    /// <summary>
    /// Inherits from the <see cref="ExpressionVisitor"/> base class and implements a expression visitor
    /// that gets a <see cref="PropertyInfo"/> that represents the property representd by the expresion.
    /// </summary>
    public class MemberAccessPropertyInfoVisitor : ExpressionVisitor
    {
        #region properties
        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> that the expression represents.
        /// </summary>
        public PropertyInfo Property { get; private set; }
        #endregion

        #region methods
        /// <summary>
        /// Overriden. Overrides all MemberAccess to build a path string.
        /// </summary>
        /// <param name="methodExp"></param>
        /// <returns></returns>
        protected override Expression VisitMemberAccess(MemberExpression methodExp)
        {
            if (methodExp.Member.MemberType != MemberTypes.Property)
                throw new NotSupportedException("MemberAccessPathVisitor does not support a member access of type " +
                                                methodExp.Member.MemberType.ToString());
            this.Property = (PropertyInfo)methodExp.Member;
            return base.VisitMemberAccess(methodExp);
        }
        #endregion
    }
}