using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using SmartStore.ConfigurableExportTest.Filters;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data;
using SmartStore.Web.Controllers;

namespace SmartStore.ConfigurableExportTest
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {


            builder.RegisterType<SampleActionFilter>()
                .AsActionFilterFor<ProductController>(x => x.ProductDetails(default(int), default(string), null))
                .InstancePerRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
