using Autofac;

namespace SmartStore.Core.Infrastructure.DependencyManagement
{
    public interface IDependencyRegistrar
    {
        /// <summary>
        /// Lets the implementor register dependencies withing the global dependency container
        /// </summary>
        /// <param name="builder">The container builder instance</param>
        /// <param name="typeFinder">The type finder instance with wich all application types can be reflected</param>
        /// <param name="isActiveModule">
		/// Indicates, whether the assembly containing this registrar instance is an active (installed) plugin assembly.
		/// The value is always <c>true</c>, if the containing assembly is not a plugin type.
		/// </param>
		void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule);

        int Order { get; }
    }
}
