using Autofac;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;

namespace SmartStore.AdManager
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        int IDependencyRegistrar.Order => 1;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            
        }

        void IDependencyRegistrar.Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            
        }
    }
}
