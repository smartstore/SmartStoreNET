using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Mvc;
using SmartStore.Utilities;
using SmartStore.Core.Caching;

namespace SmartStore.Core.Infrastructure.DependencyManagement
{
    public class ContainerManager
    {
        private readonly IContainer _container;

        public ContainerManager(IContainer container)
        {
            _container = container;
        }

        public IContainer Container
        {
            get { return _container; }
        }

        public void AddComponent<TService>(string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            AddComponent<TService, TService>(key, lifeStyle);
        }

        public void AddComponent(Type service, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            AddComponent(service, service, key, lifeStyle);
        }

        public void AddComponent<TService, TImplementation>(string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            AddComponent(typeof(TService), typeof(TImplementation), key, lifeStyle);
        }

        public void AddComponent(Type service, Type implementation, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            UpdateContainer(x =>
            {
                var serviceTypes = new List<Type> { service };

                if (service.IsGenericType)
                {
                    var temp = x.RegisterGeneric(implementation).As(
                        serviceTypes.ToArray()).PerLifeStyle(lifeStyle);
                    if (!string.IsNullOrEmpty(key))
                    {
                        temp.Keyed(key, service);
                    }
                }
                else
                {
                    var temp = x.RegisterType(implementation).As(
                        serviceTypes.ToArray()).PerLifeStyle(lifeStyle);
                    if (!string.IsNullOrEmpty(key))
                    {
                        temp.Keyed(key, service);
                    }
                }
            });
        }

        public void AddComponentInstance<TService>(TService instance, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            AddComponentInstance(typeof(TService), instance, key, lifeStyle);
        }

        public void AddComponentInstance(Type service, object instance, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            UpdateContainer(x =>
            {
                var registration = x.RegisterInstance(instance).Keyed(key, service).As(service).PerLifeStyle(lifeStyle);
            });
        }

        public void AddComponentInstance(object instance, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            AddComponentInstance(instance.GetType(), instance, key, lifeStyle);
        }

        public void AddComponentWithParameters<TService, TImplementation>(IDictionary<string, string> properties, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            AddComponentWithParameters(typeof(TService), typeof(TImplementation), properties);
        }

        public void AddComponentWithParameters(Type service, Type implementation, IDictionary<string, string> properties, string key = "", ComponentLifeStyle lifeStyle = ComponentLifeStyle.Singleton)
        {
            UpdateContainer(x =>
            {
                var serviceTypes = new List<Type> { service };

                var temp = x.RegisterType(implementation).As(serviceTypes.ToArray()).
                    WithParameters(properties.Select(y => new NamedParameter(y.Key, y.Value)));
                if (!string.IsNullOrEmpty(key))
                {
                    temp.Keyed(key, service);
                }
            });
        }

		public T Resolve<T>(string key = "", ILifetimeScope scope = null) where T : class
        {
            if (string.IsNullOrEmpty(key))
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

		public T[] ResolveAll<T>(string key = "", ILifetimeScope scope = null)
        {
            if (string.IsNullOrEmpty(key))
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
            var constructors = type.GetConstructors();
            foreach (var constructor in constructors)
            {
                try
                {
                    var parameters = constructor.GetParameters();
                    var parameterInstances = new List<object>();
                    foreach (var parameter in parameters)
                    {
                        var service = Resolve(parameter.ParameterType, scope);
                        if (service == null)
                            throw new SmartException("Unkown dependency");
                        parameterInstances.Add(service);
                    }
                    return Activator.CreateInstance(type, parameterInstances.ToArray());
                }
                catch (SmartException)
                {

                }
            }
            throw new SmartException("No contructor was found that had all the dependencies satisfied.");
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


        public void UpdateContainer(Action<ContainerBuilder> action)
        {
            var builder = new ContainerBuilder();
            action.Invoke(builder);
            builder.Update(_container);
        }

        public ILifetimeScope Scope()
        {
			ILifetimeScope scope = null;
			try
			{
				scope = AutofacDependencyResolver.Current.RequestLifetimeScope;
			}
			catch { }

			if (scope == null)
			{
				// really hackisch. But strange things are going on ?? :-)
				scope = _container.BeginLifetimeScope("AutofacWebRequest");
			}

			return scope ?? _container;
        }

    }

    public static class ContainerManagerExtensions
    {
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PerLifeStyle<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder, ComponentLifeStyle lifeStyle)
        {
            switch (lifeStyle)
            {
                case ComponentLifeStyle.LifetimeScope:
                    return HttpContext.Current != null ? builder.InstancePerHttpRequest() : builder.InstancePerLifetimeScope();
                case ComponentLifeStyle.Transient:
                    return builder.InstancePerDependency();
                case ComponentLifeStyle.Singleton:
                    return builder.SingleInstance();
                default:
                    return builder.SingleInstance();
            }
        }

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithStaticCache<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration) where TReflectionActivatorData : ReflectionActivatorData
		{
			return registration.WithParameter(Autofac.Core.ResolvedParameter.ForNamed<ICacheManager>("static"));
		}

		public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithRequestCache<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration) where TReflectionActivatorData : ReflectionActivatorData
		{
			return registration.WithParameter(Autofac.Core.ResolvedParameter.ForNamed<ICacheManager>("request"));
		}

    }
}
