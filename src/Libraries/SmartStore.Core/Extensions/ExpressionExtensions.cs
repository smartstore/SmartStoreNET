using System;
using System.Linq.Expressions;
using System.Reflection;
using SmartStore.ComponentModel;

namespace SmartStore
{
    public static class ExpressionExtensions
    {
        public static FastPropertyInvoker<T, TProp> CompileFast<T, TProp>(
            this Expression<Func<T, TProp>> expression,
            PropertyCachingStrategy cachingStrategy = PropertyCachingStrategy.Cached)
        {
            if (!(expression.Body is MemberExpression member))
            {
                throw new ArgumentException($"Expression body must refer to a property.", nameof(expression));
            }

            if (!(member.Member is PropertyInfo pi))
            {
                throw new ArgumentException($"Expression body member must refer to a property.", nameof(expression));
            }

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
