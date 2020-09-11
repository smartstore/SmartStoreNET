using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SmartStore
{
    public static class LinqExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyInfo ExtractPropertyInfo(this LambdaExpression propertyAccessor)
        {
            return propertyAccessor.ExtractMemberInfo() as PropertyInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldInfo ExtractFieldInfo(this LambdaExpression propertyAccessor)
        {
            return propertyAccessor.ExtractMemberInfo() as FieldInfo;
        }

        [SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
        public static MemberInfo ExtractMemberInfo(this LambdaExpression propertyAccessor)
        {
            if (propertyAccessor == null)
                throw new ArgumentNullException(nameof(propertyAccessor));

            MemberInfo info;
            try
            {
                MemberExpression operand;
                // o => o.PropertyOrField
                LambdaExpression expression = propertyAccessor;
                // If the property is not an Object, then the member access expression will be wrapped in a conversion expression
                // (object)o.PropertyOrField

                if (expression.Body is UnaryExpression body)
                {
                    // o.PropertyOrField
                    operand = (MemberExpression)body.Operand;
                }
                else
                {
                    // o.PropertyOrField
                    operand = (MemberExpression)expression.Body;
                }

                // Member
                MemberInfo member = operand.Member;
                info = member;
            }
            catch (Exception e)
            {
                throw new ArgumentException("The property or field accessor expression is not in the expected format 'o => o.PropertyOrField'.", e);
            }

            return info;
        }

    }

}
