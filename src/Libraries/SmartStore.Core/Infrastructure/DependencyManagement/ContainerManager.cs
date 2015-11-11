using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using SmartStore.Core.Caching;
using SmartStore.Utilities.Reflection;

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

        public IContainer Container
        {
            get { return _container; }
        }

		public T Resolve<T>(object key = null, ILifetimeScope scope = null) where T : class
        {
            if (key == null)
            {
				return (scope ?? Scope()).Resolve<T>();
            }
			return (scope ?? Scope()).ResolveKeyed<T>(key);
        }

		public T ResolveNamed<T>(string name, ILifetimeScope scope = null) where T : class
        {
			return (scope ?? Scope()).ResolveNamed<T>(name);
        }

		public object Resolve(Type type, ILifetimeScope scope = null)
        {
			return (scope ?? Scope()).Resolve(type);
        }

		public object ResolveNamed(string name, Type type, ILifetimeScope scope = null)
        {
			return (scope ?? Scope()).ResolveNamed(name, type);
        }

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
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            foreach (var constructor in constructors)
            {
				object instance;
				if (TryInvokeConstructor(constructor, out instance, scope))
				{
					return instance;
				}
            }

            throw new SmartException("No contructor was found that had all the dependencies satisfied.");
        }

		private FastActivator GetActivatorForUnregisteredService(Type serviceType)
		{
			FastActivator activator;
			if (!_cachedActivators.TryGetValue(serviceType, out activator))
			{

			}

			return activator;
		}

		private bool TryInvokeConstructor(ConstructorInfo constructor, out object instance, ILifetimeScope scope = null)
		{
			instance = null;

			try
			{
				var parameters = constructor.GetParameters();
				var parameterInstances = new List<object>();
				
                foreach (var parameter in parameters)
				{
					var service = Resolve(parameter.ParameterType, scope);
					if (service == null)
					{
						return false;
					}

					parameterInstances.Add(service);
				}

				instance = constructor.Invoke(parameterInstances.ToArray());

				return instance != null;
			}
			catch
			{
				return false;
			}
		}

		public bool TryResolve(Type serviceType, ILifetimeScope scope, out object instance)
        {
			return (scope ?? Scope()).TryResolve(serviceType, out instance);
        }

		public bool TryResolve<T>(ILifetimeScope scope, out T instance)
		{
			return (scope ?? Scope()).TryResolve<T>(out instance);
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

        public ILifetimeScope Scope()
        {
			var scope = _container.Resolve<ILifetimeScopeAccessor>().GetLifetimeScope(null);
			return scope ?? _container;
        }

		public ILifetimeScopeAccessor ScopeAccessor
		{
			get
			{
				return _container.Resolve<ILifetimeScopeAccessor>();
			}
		}

    }

    public static class ContainerManagerExtensions
    {
		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithStaticCache<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration) where TReflectionActivatorData : ReflectionActivatorData
		{
			return registration.WithParameter(Autofac.Core.ResolvedParameter.ForNamed<ICacheManager>("static"));
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithAspNetCache<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration) where TReflectionActivatorData : ReflectionActivatorData
		{
			return registration.WithParameter(Autofac.Core.ResolvedParameter.ForNamed<ICacheManager>("aspnet"));
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithNullCache<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration) where TReflectionActivatorData : ReflectionActivatorData
		{
			return registration.WithParameter(Autofac.Core.ResolvedParameter.ForNamed<ICacheManager>("null"));
		}

    }
}
