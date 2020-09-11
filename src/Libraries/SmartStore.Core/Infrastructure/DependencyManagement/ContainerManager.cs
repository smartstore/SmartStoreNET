using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Autofac;
using Autofac.Builder;
using SmartStore.ComponentModel;
using SmartStore.Core.Caching;
using SmartStore.Core.Logging;

namespace SmartStore.Core.Infrastructure.DependencyManagement
{
    public class ContainerManager
    {
        private readonly IContainer _container;
        private readonly ConcurrentDictionary<Type, FastActivator> _cachedActivators = new ConcurrentDictionary<Type, FastActivator>();

        public ContainerManager(IContainer container)
        {
            _container = container;
        }

        public IContainer Container => _container;

        [DebuggerStepThrough]
        public T Resolve<T>(object key = null, ILifetimeScope scope = null) where T : class
        {
            if (key == null)
            {
                return (scope ?? Scope()).Resolve<T>();
            }

            return (scope ?? Scope()).ResolveKeyed<T>(key);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ResolveNamed<T>(string name, ILifetimeScope scope = null) where T : class
        {
            return (scope ?? Scope()).ResolveNamed<T>(name);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Resolve(Type type, ILifetimeScope scope = null)
        {
            return (scope ?? Scope()).Resolve(type);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object ResolveNamed(string name, Type type, ILifetimeScope scope = null)
        {
            return (scope ?? Scope()).ResolveNamed(name, type);
        }

        [DebuggerStepThrough]
        public T[] ResolveAll<T>(object key = null, ILifetimeScope scope = null)
        {
            if (key == null)
            {
                return (scope ?? Scope()).Resolve<IEnumerable<T>>().ToArray();
            }

            return (scope ?? Scope()).ResolveKeyed<IEnumerable<T>>(key).ToArray();
        }

        public T ResolveUnregistered<T>(ILifetimeScope scope = null) where T : class
        {
            return ResolveUnregistered(typeof(T), scope) as T;
        }

        public object ResolveUnregistered(Type type, ILifetimeScope scope = null)
        {
            object[] parameterInstances = null;

            if (!_cachedActivators.TryGetValue(type, out FastActivator activator))
            {
                var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var constructor in constructors)
                {
                    var parameterTypes = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
                    if (TryResolveAll(parameterTypes, out parameterInstances, scope))
                    {
                        activator = new FastActivator(constructor);
                        _cachedActivators.TryAdd(type, activator);
                        break;
                    }
                }
            }

            if (activator != null)
            {
                if (parameterInstances == null)
                {
                    TryResolveAll(activator.ParameterTypes, out parameterInstances, scope);
                }

                if (parameterInstances != null)
                {
                    return activator.Activate(parameterInstances);
                }
            }

            throw new SmartException("No constructor for {0} was found that had all the dependencies satisfied.".FormatInvariant(type.Name.NaIfEmpty()));
        }

        private bool TryResolveAll(Type[] types, out object[] instances, ILifetimeScope scope = null)
        {
            instances = null;

            try
            {
                var instances2 = new object[types.Length];

                for (int i = 0; i < types.Length; i++)
                {
                    var service = Resolve(types[i], scope);
                    if (service == null)
                    {
                        return false;
                    }

                    instances2[i] = service;
                }

                instances = instances2;
                return true;
            }
            catch (Exception ex)
            {
                _container.Resolve<ILoggerFactory>().GetLogger(this.GetType()).Error(ex);
                return false;
            }
        }

        [DebuggerStepThrough]
        public bool TryResolve(Type serviceType, ILifetimeScope scope, out object instance)
        {
            instance = null;

            try
            {
                return (scope ?? Scope()).TryResolve(serviceType, out instance);
            }
            catch
            {
                return false;
            }
        }

        [DebuggerStepThrough]
        public bool TryResolve<T>(ILifetimeScope scope, out T instance)
            where T : class
        {
            instance = default(T);

            try
            {
                return (scope ?? Scope()).TryResolve<T>(out instance);
            }
            catch
            {
                return false;
            }
        }

        public bool IsRegistered(Type serviceType, ILifetimeScope scope = null)
        {
            return (scope ?? Scope()).IsRegistered(serviceType);
        }

        public object ResolveOptional(Type serviceType, ILifetimeScope scope = null)
        {
            return (scope ?? Scope()).ResolveOptional(serviceType);
        }

        public T InjectProperties<T>(T instance, ILifetimeScope scope = null)
        {
            return (scope ?? Scope()).InjectProperties(instance);
        }

        public T InjectUnsetProperties<T>(T instance, ILifetimeScope scope = null)
        {
            return (scope ?? Scope()).InjectUnsetProperties(instance);
        }

        [DebuggerStepThrough]
        public ILifetimeScope Scope()
        {
            var scope = _container.Resolve<ILifetimeScopeAccessor>().GetLifetimeScope(null);
            return scope ?? _container;
        }

        public ILifetimeScopeAccessor ScopeAccessor => _container.Resolve<ILifetimeScopeAccessor>();

    }

    public static class ContainerManagerExtensions
    {
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithNullCache<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration) where TReflectionActivatorData : ReflectionActivatorData
        {
            return registration.WithParameter(Autofac.Core.ResolvedParameter.ForNamed<ICacheManager>("null"));
        }

    }
}
