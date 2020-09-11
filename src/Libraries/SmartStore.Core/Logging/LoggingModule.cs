using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;
using SmartStore.ComponentModel;
using SmartStore.Core.Data;

namespace SmartStore.Core.Logging
{
    public class LoggingModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NullChronometer>().As<IChronometer>().SingleInstance();

            builder.RegisterType<Log4netLoggerFactory>().As<ILoggerFactory>().SingleInstance();

            // call GetLogger in response to the request for an ILogger implementation
            if (DataSettings.DatabaseIsInstalled())
            {
                builder.Register(GetContextualLogger).As<ILogger>().ExternallyOwned();
            }
            else
            {
                // the install logger should append to a rolling text file only.
                builder.Register(GetInstallLogger).As<ILogger>().ExternallyOwned();
            }
        }

        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            bool hasCtorLogger = false;
            bool hasPropertyLogger = false;

            FastProperty[] loggerProperties = null;

            var ra = registration.Activator as ReflectionActivator;
            if (ra != null)
            {
                // // Look for ctor parameters of type "ILogger" 
                var ctors = ra.ConstructorFinder.FindConstructors(ra.LimitType);
                var loggerParameters = ctors.SelectMany(ctor => ctor.GetParameters()).Where(pi => pi.ParameterType == typeof(ILogger));
                hasCtorLogger = loggerParameters.Any();

                // Autowire properties
                // Look for settable properties of type "ILogger" 
                loggerProperties = ra.LimitType
                    .GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => new
                    {
                        PropertyInfo = p,
                        p.PropertyType,
                        IndexParameters = p.GetIndexParameters(),
                        Accessors = p.GetAccessors(false)
                    })
                    .Where(x => x.PropertyType == typeof(ILogger)) // must be a logger
                    .Where(x => x.IndexParameters.Count() == 0) // must not be an indexer
                    .Where(x => x.Accessors.Length != 1 || x.Accessors[0].ReturnType == typeof(void)) //must have get/set, or only set
                    .Select(x => FastProperty.Create(x.PropertyInfo))
                    .ToArray();

                hasPropertyLogger = loggerProperties.Length > 0;

                // Ignore components known to be without logger dependencies
                if (!hasCtorLogger && !hasPropertyLogger)
                    return;

                if (hasPropertyLogger)
                {
                    registration.Metadata.Add("LoggerProperties", loggerProperties);
                }
            }

            if (hasCtorLogger)
            {
                registration.Preparing += (sender, args) =>
                {
                    var logger = GetLoggerFor(args.Component.Activator.LimitType, args.Context);
                    args.Parameters = new[] { TypedParameter.From(logger) }.Concat(args.Parameters);
                };
            }

            if (hasPropertyLogger)
            {
                registration.Activating += (sender, args) =>
                {
                    var logger = GetLoggerFor(args.Component.Activator.LimitType, args.Context);
                    var loggerProps = args.Component.Metadata.Get("LoggerProperties") as FastProperty[];
                    if (loggerProps != null)
                    {
                        foreach (var prop in loggerProps)
                        {
                            prop.SetValue(args.Instance, logger);
                        }
                    }
                };
            }
        }

        private ILogger GetLoggerFor(Type componentType, IComponentContext ctx)
        {
            return ctx.Resolve<ILogger>(new TypedParameter(typeof(Type), componentType));
        }

        private static ILogger GetContextualLogger(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            // return an ILogger in response to Resolve<ILogger>(componentTypeParameter)
            var loggerFactory = context.Resolve<ILoggerFactory>();

            Type containingType = null;

            if (parameters != null && parameters.Any())
            {
                if (parameters.Any(x => x is TypedParameter))
                {
                    containingType = parameters.TypedAs<Type>();
                }
                else if (parameters.Any(x => x is NamedParameter))
                {
                    containingType = parameters.Named<Type>("Autofac.AutowiringPropertyInjector.InstanceType");
                }
            }

            if (containingType != null)
            {
                return loggerFactory.GetLogger(containingType);
            }
            else
            {
                return loggerFactory.GetLogger("SmartStore");
            }
        }

        private static ILogger GetInstallLogger(IComponentContext context)
        {
            return context.Resolve<ILoggerFactory>().GetLogger("Install");
        }
    }
}
