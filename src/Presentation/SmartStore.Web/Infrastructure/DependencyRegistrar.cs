using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Services.Installation;
using SmartStore.Web.Controllers;
using SmartStore.Web.Infrastructure.Installation;

namespace SmartStore.Web.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            //we cache presentation models between requests
            builder.RegisterType<BlogController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"));
            builder.RegisterType<CatalogController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"));
            builder.RegisterType<CountryController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"));
            builder.RegisterType<CommonController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"));
            builder.RegisterType<NewsController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"));
            builder.RegisterType<PollController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"));
            builder.RegisterType<ShoppingCartController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"));
            builder.RegisterType<TopicController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"));
            
            //installation localization service
            builder.RegisterType<InstallationLocalizationService>().As<IInstallationLocalizationService>().InstancePerHttpRequest();

            // codehint: sm-add
            // register app languages for installation
            builder.RegisterType<EnUSInstallationData>()
                .As<InvariantInstallationData>()
                .WithMetadata<InstallationAppLanguageMetadata>(m =>
                { 
                    m.For(em => em.Culture, "en-US");
                    m.For(em => em.Name, "English");
                    m.For(em => em.UniqueSeoCode, "en");
                    m.For(em => em.FlagImageFileName, "us.png");
                })
                .InstancePerHttpRequest();
            builder.RegisterType<DeDEInstallationData>()
                .As<InvariantInstallationData>()
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
