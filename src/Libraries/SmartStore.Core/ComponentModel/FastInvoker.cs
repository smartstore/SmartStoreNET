using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SmartStore.Utilities;

namespace SmartStore.ComponentModel
{
    public class FastInvoker
    {
        private static readonly ConcurrentDictionary<MethodKey, FastInvoker> _invokersCache = new ConcurrentDictionary<MethodKey, FastInvoker>();

        public FastInvoker(MethodInfo methodInfo)
        {
            Guard.NotNull(methodInfo, nameof(methodInfo));

            Method = methodInfo;
            Invoker = MakeFastInvoker(methodInfo);
            ParameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
        }

        /// <summary>
        /// Gets the backing <see cref="MethodInfo"/>.
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        /// Gets the parameter types from the backing <see cref="MethodInfo"/>
        /// </summary>
        public Type[] ParameterTypes { get; private set; }

        /// <summary>
        /// Gets the method invoker.
        /// </summary>
        public Func<object, object[], object> Invoker { get; private set; }

        /// <summary>
        /// Invokes the method using the specified parameters.
        /// </summary>
        /// <returns>The method invocation result.</returns>
        public object Invoke(object obj, params object[] parameters)
        {
            return Invoker(obj, parameters);
        }

        #region Static

        /// <summary>
        /// Creates a single fast method invoker. The result is not cached.
        /// </summary>
        /// <param name="method">Method to create invoker for.</param>
        /// <returns>The fast method invoker delegate.</returns>
        public static Func<object, object[], object> MakeFastInvoker(MethodInfo method)
        {
            Guard.NotNull(method, nameof(method));

            var instanceParameterExpression = Expression.Parameter(typeof(object), "instance");
            var argumentsParameterExpression = Expression.Parameter(typeof(object[]), "args");
            var index = 0;

            var argumentExtractionExpressions =
                method
               .GetParameters()
               .Select(parameter =>
                  Expression.Convert(
                     Expression.ArrayAccess(
                        argumentsParameterExpression,
                        Expression.Constant(index++)
                     ),
                     parameter.ParameterType
                  )
               ).ToList();

            var callExpression = method.IsStatic
               ? Expression.Call(method, argumentExtractionExpressions)
               : Expression.Call(
                  Expression.Convert(
                     instanceParameterExpression,
                     method.DeclaringType
                  ),
                  method,
                  argumentExtractionExpressions
               );

            var endLabel = Expression.Label(typeof(object));

            var finalExpression = method.ReturnType == typeof(void)
               ? (Expression)Expression.Block(
                    callExpression,
                    Expression.Return(endLabel, Expression.Constant(null)),
                    Expression.Label(endLabel, Expression.Constant(null))
                 )
               : Expression.Convert(callExpression, typeof(object));

            var lambdaExpression = Expression.Lambda<Func<object, object[], object>>(
               finalExpression,
               instanceParameterExpression,
               argumentsParameterExpression
            );

            var lamdba = lambdaExpression.Compile();
            return lamdba;
        }

        /// <summary>
        /// Invokes a method using the specified object and parameter instances.
        /// </summary>
        /// <param name="obj">The objectinstance</param>
        /// <param name="methodName">Method name</param>
        /// <param name="parameterTypes">Argument types of the matching method overload (in exact order)</param>
        /// <param name="parameters">Parameter instances to pass to invocation</param>
        /// <returns>The method invocation result.</returns>
        public static object Invoke(object obj, string methodName, Type[] parameterTypes, object[] parameters)
        {
            Guard.NotNull(obj, nameof(obj));

            FastInvoker invoker;

            if (parameterTypes == null || parameterTypes.Length == 0)
            {
                invoker = GetInvoker(obj.GetType(), methodName);
            }
            else
            {
                invoker = GetInvoker(obj.GetType(), methodName, parameterTypes);
            }

            //var hasAnyNullParam = parameters.Any(x => x == null);
            //if (hasAnyNullParam)
            //{
            //	throw new ArgumentException("When invoking a method with parameter instances, no instance must be null.", nameof(parameters));
            //}

            return invoker.Invoke(obj, parameters ?? Array.Empty<object>());
        }

        /// <summary>
        /// Creates and caches a fast method invoker.
        /// </summary>
        /// <param name="methodName">Name of method to create an invoker for.</param>
        /// <param name="argTypes">Argument types of method to create an invoker for.</param>
        /// <returns>The fast method invoker.</returns>
        public static FastInvoker GetInvoker<T>(string methodName, params Type[] argTypes)
        {
            return GetInvoker(typeof(T), methodName, argTypes);
        }

        /// <summary>
        /// Creates and caches a fast method invoker.
        /// </summary>
        /// <param name="type">The type to extract fast method invoker for.</param>
        /// <param name="methodName">Name of method to create an invoker for.</param>
        /// <param name="argTypes">Argument types of method to create an invoker for.</param>
        /// <returns>The fast method invoker.</returns>
        public static FastInvoker GetInvoker(Type type, string methodName, params Type[] argTypes)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotEmpty(methodName, nameof(methodName));

            var cacheKey = MethodKey.Create(type, methodName, argTypes);

            if (!_invokersCache.TryGetValue(cacheKey, out var invoker))
            {
                var method = FindMatchingMethod(type, methodName, argTypes);

                if (method == null)
                {
                    throw new MethodAccessException("Could not find a matching method '{0}' in type {1}.".FormatInvariant(methodName, type));
                }

                invoker = new FastInvoker(method);
                _invokersCache.TryAdd(cacheKey, invoker);
            }

            return invoker;
        }

        /// <summary>
        /// Creates and caches a fast method invoker.
        /// </summary>
        /// <param name="method">Method info instance to create an invoker for.</param>
        /// <returns>The fast method invoker.</returns>
        public static FastInvoker GetInvoker(MethodInfo method)
        {
            Guard.NotNull(method, nameof(method));

            var cacheKey = MethodKey.Create(method);

            if (!_invokersCache.TryGetValue(cacheKey, out var invoker))
            {
                invoker = new FastInvoker(method);
                _invokersCache.TryAdd(cacheKey, invoker);
            }

            return invoker;
        }

        private static MethodInfo FindMatchingMethod(Type type, string methodName, Type[] argTypes)
        {
            var method = argTypes == null || argTypes.Length == 0
                ? type.GetMethod(methodName)
                : type.GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                    null,
                    argTypes ?? new Type[0],
                    null);

            return method;
        }

        #endregion

        abstract class MethodKey
        {
            public override bool Equals(object obj)
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public static bool operator ==(MethodKey left, MethodKey right)
            {
                return object.Equals(left, right);
            }

            public static bool operator !=(MethodKey left, MethodKey right)
            {
                return !(left == right);
            }

            internal static MethodKey Create(Type type, string methodName, IEnumerable<Type> parameterTypes)
            {
                return new HashMethodKey(type, methodName, parameterTypes);
            }

            internal static MethodKey Create(MethodInfo method)
            {
                return new MethodInfoKey(method);
            }
        }

        class HashMethodKey : MethodKey, IEquatable<HashMethodKey>
        {
            private readonly int _hash;

            public HashMethodKey(Type type, string methodName, IEnumerable<Type> parameterTypes)
            {
                _hash = HashCodeCombiner.Start().Add(type).Add(methodName).Add(parameterTypes).CombinedHash;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as HashMethodKey);
            }

            public bool Equals(HashMethodKey other)
            {
                if (other == null)
                    return false;

                return this._hash == other._hash;
            }

            public override int GetHashCode()
            {
                return _hash;
            }
        }

        class MethodInfoKey : MethodKey, IEquatable<MethodInfoKey>
        {
            private readonly MethodInfo _method;

            public MethodInfoKey(MethodInfo method)
            {
                _method = method;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as MethodInfoKey);
            }

            public bool Equals(MethodInfoKey other)
            {
                if (other == null)
                    return false;

                return this._method == other._method;
            }

            public override int GetHashCode()
            {
                return _method.GetHashCode();
            }
        }
    }
}
