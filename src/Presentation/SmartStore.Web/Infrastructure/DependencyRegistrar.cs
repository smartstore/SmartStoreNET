using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data.Setup;
using SmartStore.Web.Controllers;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Installation;

namespace SmartStore.Web.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            //we cache presentation models between requests
			builder.RegisterType<BlogController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("static"));
			builder.RegisterType<CatalogController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("static"));
			builder.RegisterType<CountryController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("static"));
			builder.RegisterType<CommonController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("static"));
			builder.RegisterType<NewsController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("static"));
			builder.RegisterType<PollController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("static"));
			builder.RegisterType<ShoppingCartController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("static"));
			builder.RegisterType<TopicController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("static"));

            builder.RegisterType<DefaultWidgetSelector>().As<IWidgetSelector>().InstancePerHttpRequest();
            
            // installation localization service
            builder.RegisterType<InstallationLocalizationService>().As<IInstallationLocalizationService>().InstancePerHttpRequest();

            // register app languages for installation
			builder.RegisterType<EnUSSeedData>()
                .As<InvariantSeedData>()
                .WithMetadata<InstallationAppLanguageMetadata>(m =>
                { 
                    m.For(em => em.Culture, "en-US");
                    m.For(em => em.Name, "English");
                    m.For(em => em.UniqueSeoCode, "en");
                    m.For(em => em.FlagImageFileName, "us.png");
                })
                .InstancePerHttpRequest();
            builder.RegisterType<DeDESeedData>()
				.As<InvariantSeedData>()
                .WithMetadata<InstallationAppLanguageMetadata>(m =>
                {
                    m.For(em => em.Culture, "de-DE");
                    m.For(em => em.Name, "Deutsch");
                    m.For(em => em.UniqueSeoCode, "de");
                    m.For(em => em.FlagImageFileName, "de.png");
                })
                .InstancePerHttpRequest();
        }

        public int Order
        {
            get { return 2; }
        }
    }
}
