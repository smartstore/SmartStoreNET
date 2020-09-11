using System;
using Autofac;

namespace SmartStore.Rules
{
    public class RuleModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RuleStorage>().As<IRuleStorage>().InstancePerRequest();
            builder.RegisterType<RuleFactory>().As<IRuleFactory>().InstancePerRequest();

            // Rendering.
            builder.RegisterType<RuleTemplateSelector>().As<IRuleTemplateSelector>().InstancePerRequest();

            // Register provider resolver delegate.
            builder.Register<Func<RuleScope, IRuleProvider>>(c =>
            {
                // TODO: register providers explicitly
                var cc = c.Resolve<IComponentContext>();
                return key => cc.ResolveKeyed<IRuleProvider>(key);
            });
        }
    }
}
