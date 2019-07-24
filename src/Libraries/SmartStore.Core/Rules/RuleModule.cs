using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using SmartStore.ComponentModel;
using SmartStore.Core.Data;

namespace SmartStore.Rules
{
	public class RuleModule : Autofac.Module
	{
		protected override void Load(ContainerBuilder builder)
		{
            builder.RegisterType<RuleStorage>().As<IRuleStorage>().InstancePerRequest();
            builder.RegisterType<RuleFactory>().As<IRuleFactory>().InstancePerRequest();

            // Register provider resolver delegate
            builder.Register<Func<RuleScope, IRuleProvider>>(c =>
            {
                // TODO: register providers explicitly
                var cc = c.Resolve<IComponentContext>();
                return key => cc.ResolveKeyed<IRuleProvider>(key);
            });
        }
	}
}
