using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Admin.Controllers;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Admin.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            builder.RegisterType<AdminModelHelper>().InstancePerRequest();

            if (DataSettings.DatabaseIsInstalled())
            {
                builder.RegisterType<PreviewModeFilter>().AsResultFilterFor<PublicControllerBase>();
            }
        }

        public int Order => 100;
    }
}
