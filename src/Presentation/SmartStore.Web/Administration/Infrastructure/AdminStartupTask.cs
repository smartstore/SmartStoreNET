using SmartStore.Core.Infrastructure;

namespace SmartStore.Admin.Infrastructure
{
    public class AdminStartupTask : IStartupTask
    {
        public void Execute()
        {
            // codehint: sm-delete (Telerik internal localization works better for whatever reason)
            ////set localization service for telerik
            //Telerik.Web.Mvc.Infrastructure.DI.Current.Register(
            //    () => EngineContext.Current.Resolve<Telerik.Web.Mvc.Infrastructure.ILocalizationServiceFactory>());
        }

        public int Order
        {
            get { return 100; }
        }
    }
}