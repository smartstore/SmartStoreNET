using System;
using System.Linq.Expressions;
using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Plugin.ExternalAuth.Facebook.Core;

namespace SmartStore.Plugin.ExternalAuth.Facebook
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            builder.RegisterType<FacebookProviderAuthorizer>().As<IOAuthProviderFacebookAuthorizer>().InstancePerRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
