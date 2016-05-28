using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;


/*
 * NOTE: This class was re-used from the NHibernate.Linq project.
 */

namespace SmartStore.Linq.Expressions
{

    /// <summary>
    /// Provides virtual methods that can be used by subclasses to parse an expression tree.
    /// </summary>
    /// <remarks>
    /// This class actually already exists in the System.Core assembly...as an internal class.
    /// I can only speculate as to why it is internal, but it is obviously much too dangerous
    /// for anyone outside of Microsoft to be using...
    /// </remarks>
    [DebuggerStepThrough, DebuggerNonUserCode]
    [SuppressMessage("ReSharper", "PossibleUnintendedReferenceComparison")]
    public abstract class ExpressionVisitor
    {
        ///<summary>
        /// Visits and performs transaction on an expression.
        ///</summary>
        ///<param name="exp">The <see cref="Expression"/> to visit and transform.</param>
        ///<returns>A transformed <see cref="Expression"/> instance.</returns>
        ///<exception cref="NotSupportedException">Throws when a expression or sub-expression is not supported
        /// by the Expression visitor.</exception>
        public virtual Expression Visit(Expression exp)
        {
            if (exp == null) return exp;

            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression)exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)exp);
                case ExpressionType.TypeIs:
                    return VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)exp);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)exp);
                default:
                    throw new NotSupportedException(String.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        /// <summary>
        /// Visit a bindingExp expression.
        /// </summary>
        /// <param name="bindingExp"></param>
        /// <returns></returns>
        protected virtual MemberBinding VisitBinding(MemberBinding bindingExp)
        {
            switch (bindingExp.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)bindingExp);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)bindingExp);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)bindingExp);
                default:
                    throw new NotSupportedException(string.Format("Unhandled binding type '{0}'", bindingExp.BindingType));
            }
        }

        /// <summary>
        /// Visit a element initialized expression.
        /// </summary>
        /// <param name="elementInitExp"></param>
        /// <returns></returns>
        protected virtual ElementInit VisitElementInitializer(ElementInit elementInitExp)
        {
            ReadOnlyCollection<Expression> arguments = VisitList(elementInitExp.Arguments);
            if (arguments != elementInitExp.Arguments)
            {
                return Expression.ElementInit(elementInitExp.AddMethod, arguments);
            }
            return elementInitExp;
        }

        /// <summary>
        /// Visit a Unary expression.
        /// </summary>
        /// <param name="unaryExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitUnary(UnaryExpression unaryExp)
        {
            Expression operand = Visit(unaryExp.Operand);
            if (operand != unaryExp.Operand)
            {
                return Expression.MakeUnary(unaryExp.NodeType, operand, unaryExp.Type, unaryExp.Method);
            }
            return unaryExp;
        }

        /// <summary>
        /// Visit a binary expression.
        /// </summary>
        /// <param name="binaryExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitBinary(BinaryExpression binaryExp)
        {
            Expression left = Visit(binaryExp.Left);
            Expression right = Visit(binaryExp.Right);
            Expression conversion = Visit(binaryExp.Conversion);

            if (left != binaryExp.Left || right != binaryExp.Right || conversion != binaryExp.Conversion)
            {
                if (binaryExp.NodeType == ExpressionType.Coalesce && binaryExp.Conversion != null)
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                else
                    return Expression.MakeBinary(binaryExp.NodeType, left, right, binaryExp.IsLiftedToNull, binaryExp.Method);
            }
            return binaryExp;
        }

        /// <summary>
        /// Visit a Is expression.
        /// </summary>
        /// <param name="typeBinaryExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitTypeIs(TypeBinaryExpression typeBinaryExp)
        {
            Expression expr = Visit(typeBinaryExp.Expression);
            if (expr != typeBinaryExp.Expression)
            {
                return Expression.TypeIs(expr, typeBinaryExp.TypeOperand);
            }
            return typeBinaryExp;
        }

        /// <summary>
        /// Visit a constant expression.
        /// </summary>
        /// <param name="constantExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitConstant(ConstantExpression constantExp)
        {
            return constantExp;
        }

        /// <summary>
        /// Visit a conditional expression.
        /// </summary>
        /// <param name="conditionalExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitConditional(ConditionalExpression conditionalExp)
        {
            Expression test = Visit(conditionalExp.Test);
            Expression ifTrue = Visit(conditionalExp.IfTrue);
            Expression ifFalse = Visit(conditionalExp.IfFalse);

            if (test != conditionalExp.Test || ifTrue != conditionalExp.IfTrue || ifFalse != conditionalExp.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }

            return conditionalExp;
        }

        /// <summary>
        /// Visit a parameter expression.
        /// </summary>
        /// <param name="parameterExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitParameter(ParameterExpression parameterExp)
        {
            return parameterExp;
        }

        /// <summary>
        /// Visit a member access expression.
        /// </summary>
        /// <param name="methodExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitMemberAccess(MemberExpression methodExp)
        {
            Expression exp = Visit(methodExp.Expression);
            if (exp != methodExp.Expression)
            {
                return Expression.MakeMemberAccess(exp, methodExp.Member);
            }
            return methodExp;
        }

        /// <summary>
        /// Visit a method call expression.
        /// </summary>
        /// <param name="methodCallExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitMethodCall(MethodCallExpression methodCallExp)
        {
            Expression obj = Visit(methodCallExp.Object);
            IEnumerable<Expression> args = VisitList(methodCallExp.Arguments);

            if (obj != methodCallExp.Object || args != methodCallExp.Arguments)
            {
                return Expression.Call(obj, methodCallExp.Method, args);
            }

            return methodCallExp;
        }

        /// <summary>
        /// Visit a list expression.
        /// </summary>
        /// <param name="listExp"></param>
        /// <returns></returns>
        protected virtual ReadOnlyCollection<Expression> VisitList(ReadOnlyCollection<Expression> listExp)
        {
            List<Expression> list = null;
            for (int i = 0, n = listExp.Count; i < n; i++)
            {
                Expression p = Visit(listExp[i]);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != listExp[i])
                {
                    list = new List<Expression>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(listExp[j]);
                    }
                    list.Add(p);
                }
            }

            if (list != null)
                return list.AsReadOnly();

            return listExp;
        }

        /// <summary>
        /// Visit a member assignment expression.
        /// </summary>
        /// <param name="assignmentExp"></param>
        /// <returns></returns>
        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignmentExp)
        {
            Expression e = Visit(assignmentExp.Expression);

            if (e != assignmentExp.Expression)
            {
                return Expression.Bind(assignmentExp.Member, e);
            }

            return assignmentExp;
        }

        /// <summary>
        /// Visit a member binding expression.
        /// </summary>
        /// <param name="memberBindingExp"></param>
        /// <returns></returns>
        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding memberBindingExp)
        {
            IEnumerable<MemberBinding> bindings = VisitBindingList(memberBindingExp.Bindings);

            if (bindings != memberBindingExp.Bindings)
            {
                return Expression.MemberBind(memberBindingExp.Member, bindings);
            }

            return memberBindingExp;
        }

        /// <summary>
        /// Visit a member list binding expression.
        /// </summary>
        /// <param name="listBindingExp"></param>
        /// <returns></returns>
        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding listBindingExp)
        {
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(listBindingExp.Initializers);

            if (initializers != listBindingExp.Initializers)
            {
                return Expression.ListBind(listBindingExp.Member, initializers);
            }
            return listBindingExp;
        }

        /// <summary>
        /// Visint a binding list expression.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                MemberBinding b = VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }

            if (list != null)
                return list;

            return original;
        }

        /// <summary>
        /// Visit a element list initializer expression
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                ElementInit init = VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }

            if (list != null)
                return list;

            return original;
        }

        /// <summary>
        /// Visit a expression that represents a lambda.
        /// </summary>
        /// <param name="lambdaExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitLambda(LambdaExpression lambdaExp)
        {
            Expression body = Visit(lambdaExp.Body);
            if (body != lambdaExp.Body)
            {
                return Expression.Lambda(lambdaExp.Type, body, lambdaExp.Parameters);
            }
            return lambdaExp;
        }

        /// <summary>
        /// Visit a new expresson (ctor)
        /// </summary>
        /// <param name="newExp"></param>
        /// <returns></returns>
        protected virtual NewExpression VisitNew(NewExpression newExp)
        {
            IEnumerable<Expression> args = VisitList(newExp.Arguments);
            if (args != newExp.Arguments)
            {
                if (newExp.Members != null)
                    return Expression.New(newExp.Constructor, args, newExp.Members);
                else
                    return Expression.New(newExp.Constructor, args);
            }

            return newExp;
        }

        /// <summary>
        /// Visit a member initialization expression.
        /// </summary>
        /// <param name="memberInitExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitMemberInit(MemberInitExpression memberInitExp)
        {
            NewExpression n = VisitNew(memberInitExp.NewExpression);
            IEnumerable<MemberBinding> bindings = VisitBindingList(memberInitExp.Bindings);

            if (n != memberInitExp.NewExpression || bindings != memberInitExp.Bindings)
            {
                return Expression.MemberInit(n, bindings);
            }

            return memberInitExp;
        }

        /// <summary>
        /// Visit a list initializatio expression.
        /// </summary>
        /// <param name="listInitExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitListInit(ListInitExpression listInitExp)
        {
            NewExpression n = VisitNew(listInitExp.NewExpression);
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(listInitExp.Initializers);

            if (n != listInitExp.NewExpression || initializers != listInitExp.Initializers)
            {
                return Expression.ListInit(n, initializers);
            }

            return listInitExp;
        }

        /// <summary>
        /// Visit a new array expression.
        /// </summary>
        /// <param name="newArrayExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitNewArray(NewArrayExpression newArrayExp)
        {
            IEnumerable<Expression> exprs = VisitList(newArrayExp.Expressions);
            if (exprs != newArrayExp.Expressions)
            {
                if (newArrayExp.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(newArrayExp.Type.GetElementType(), exprs);
                }
                else
                {
                    return Expression.NewArrayBounds(newArrayExp.Type.GetElementType(), exprs);
                }
            }

            return newArrayExp;
        }

        /// <summary>
        /// Visit a invocation expression.
        /// </summary>
        /// <param name="invocationExp"></param>
        /// <returns></returns>
        protected virtual Expression VisitInvocation(InvocationExpression invocationExp)
        {
            IEnumerable<Expression> args = VisitList(invocationExp.Arguments);
            Expression expr = Visit(invocationExp.Expression);

            if (args != invocationExp.Arguments || expr != invocationExp.Expression)
            {
                return Expression.Invoke(expr, args);
            }

            return invocationExp;
        }
    }

}
