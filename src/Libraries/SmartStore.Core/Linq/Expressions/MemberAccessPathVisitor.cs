using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SmartStore.Linq.Expressions
{

    /// <summary>
    /// Inherits from the <see cref="ExpressionVisitor"/> base class and implements a expression visitor
    /// that builds up a path string that represents meber access in a Expression.
    /// </summary>
    public class MemberAccessPathVisitor : ExpressionVisitor
    {
        #region fields
        //StringBuilder instance that will store the path.
        private readonly Stack<string> _path = new Stack<string>();
        #endregion

        #region properties
        /// <summary>
        /// Gets the path analyzed by the visitor.
        /// </summary>
        public string Path
        {
            get
            {
                var pathString = new StringBuilder();
                foreach (string path in _path)
                {
                    if (pathString.Length == 0)
                        pathString.Append(path);
                    else
                        pathString.AppendFormat(".{0}", path);
                }
                return pathString.ToString();
            }
        }
        #endregion

        #region overriden methods
        /// <summary>
        /// Overriden. Overrides all MemberAccess to build a path string.
        /// </summary>
        /// <param name="methodExp"></param>
        /// <returns></returns>
        protected override Expression VisitMemberAccess(MemberExpression methodExp)
        {
            if (methodExp.Member.MemberType != MemberTypes.Field && methodExp.Member.MemberType != MemberTypes.Property)
                throw new NotSupportedException("MemberAccessPathVisitor does not support a member access of type " +
                                                methodExp.Member.MemberType.ToString());
            _path.Push(methodExp.Member.Name);
            return base.VisitMemberAccess(methodExp);
        }

        /// <summary>
        /// Overriden. Throws a <see cref="NotSupportedException"/> when a method call is encountered.
        /// </summary>
        /// <param name="methodCallExp"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExp)
        {
            throw new NotSupportedException(
                "MemberAccessPathVisitor does not support method calls. Only MemberAccess expressions are allowed.");
        }
        #endregion

    }

}
