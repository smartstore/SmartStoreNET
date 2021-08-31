using Autofac;
using Autofac.Core;
using SmartStore.Core.Data;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Services.Messages;
//using SmartStore.Data;

namespace Strube.Export
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order { get { return 1; } }

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            //throw new System.NotImplementedException();
            builder.RegisterType<EmailAccountService>().As<IEmailAccountService>().InstancePerRequest();
        }
    }
}