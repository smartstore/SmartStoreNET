using Autofac;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data.Setup;
using SmartStore.Services.Search.Rendering;
using SmartStore.Web.Controllers;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Installation;

namespace SmartStore.Web.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            builder.RegisterType<CatalogHelper>().InstancePerRequest();
            builder.RegisterType<OrderHelper>().InstancePerRequest();

            builder.RegisterType<DefaultWidgetSelector>().As<IWidgetSelector>().InstancePerRequest();
            builder.RegisterType<DefaultFacetTemplateSelector>().As<IFacetTemplateSelector>().SingleInstance();

            // Installation localization service
            builder.RegisterType<InstallationLocalizationService>().As<IInstallationLocalizationService>().InstancePerRequest();

            // Register app languages for installation
            builder.RegisterType<EnUSSeedData>()
                .As<InvariantSeedData>()
                .WithMetadata<InstallationAppLanguageMetadata>(m =>
                {
                    m.For(em => em.Culture, "en-US");
                    m.For(em => em.Name, "English");
                    m.For(em => em.UniqueSeoCode, "en");
                    m.For(em => em.FlagImageFileName, "us.png");
                })
                .InstancePerRequest();
            builder.RegisterType<DeDESeedData>()
                .As<InvariantSeedData>()
                .WithMetadata<InstallationAppLanguageMetadata>(m =>
                {
                    m.For(em => em.Culture, "de-DE");
                    m.For(em => em.Name, "Deutsch");
                    m.For(em => em.UniqueSeoCode, "de");
                    m.For(em => em.FlagImageFileName, "de.png");
                })
                .InstancePerRequest();
        }

        public int Order => 2;
    }
}
