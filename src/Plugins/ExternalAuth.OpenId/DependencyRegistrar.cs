using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Plugin.ExternalAuth.OpenId.Core;

namespace SmartStore.Plugin.ExternalAuth.OpenId
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<OpenIdProviderAuthorizer>().As<IOpenIdProviderAuthorizer>().InstancePerRequest();
            builder.RegisterType<OpenIdRelyingPartyService>().As<IOpenIdRelyingPartyService>().InstancePerRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
