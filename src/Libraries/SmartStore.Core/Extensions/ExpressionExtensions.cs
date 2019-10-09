using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SmartStore.ComponentModel;

namespace SmartStore
{
    public static class ExpressionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FastPropertyInvoker<T, TProp> CompileFast<T, TProp>(this Expression<Func<T, TProp>> expression)
        {
            return CompileFast(expression, PropertyCachingStrategy.Cached, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FastPropertyInvoker<T, TProp> CompileFast<T, TProp>(this Expression<Func<T, TProp>> expression, out string propertyName)
        {
            return CompileFast(expression, PropertyCachingStrategy.Cached, out propertyName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FastPropertyInvoker<T, TProp> CompileFast<T, TProp>(this Expression<Func<T, TProp>> expression, PropertyCachingStrategy cachingStrategy)
        {
            return CompileFast(expression, cachingStrategy, out _);
        }

        public static FastPropertyInvoker<T, TProp> CompileFast<T, TProp>(this Expression<Func<T, TProp>> expression, PropertyCachingStrategy cachingStrategy, out string propertyName)
        {
            propertyName = null;

            if (!(expression.Body is MemberExpression member))
            {
                throw new ArgumentException($"Expression body must refer to a property.", nameof(expression));
            }

            if (!(member.Member is PropertyInfo pi))
            {
                throw new ArgumentException($"Expression body member must refer to a property.", nameof(expression));
            }

            propertyName = pi.Name;

            var fastProp = FastProperty.GetProperty(pi, cachingStrategy);
            return new FastPropertyInvoker<T, TProp>(fastProp);
        }
    }

    public sealed class FastPropertyInvoker<T, TProp>
    {
        internal FastPropertyInvoker(FastProperty prop)
        {
            Property = prop;
        }

        public FastProperty Property { get; private set; }

        public TProp Invoke(T obj)
        {
            return (TProp)Property.GetValue(obj);
        }

        public static implicit operator Func<T, TProp>(FastPropertyInvoker<T, TProp> obj)
        {
            return obj.Invoke;
        }
    }
}
