using SmartStore.Core.Events;
using SmartStore.DevTools.Models;
using SmartStore.Services;
using SmartStore.Web.Framework.Events;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.DevTools.Events
{
    public class CustomTab : IConsumer
    {
        private readonly ICommonServices _services;

        public CustomTab(ICommonServices services)
        {
            _services = services;
        }

        public void HandleEvent(TabStripCreated eventMessage)
        {
            // add a form to product detail configuration
            if (eventMessage.TabStripName == "product-edit")
            {
                var productId = ((TabbableModel)eventMessage.Model).Id;

                // add in a predefined tab "Plugins" which serves as container for plugins to obtain data 

                //eventMessage.AddWidget(new RouteInfo(
                //    "ProductEditTab",
                //    "DevTools",
                //    new { area = "SmartStore.DevTools", productId = productId }
                //));

                // add in an own tab

                //eventMessage.ItemFactory.Add().Text("Dev Tools")
                //    .Name("tab-dt")
                //    .Icon("fa fa-code fa-lg fa-fw")
                //    .LinkHtmlAttributes(new { data_tab_name = "DevTools" })
                //    .Route("SmartStore.DevTools", new { action = "ProductEditTab", productId = productId })
                //    .Ajax();

            }
        }

        public void HandleEvent(ModelBoundEvent eventMessage)
        {
            if (!eventMessage.BoundModel.CustomProperties.ContainsKey("DevTools"))
                return;

            var model = eventMessage.BoundModel.CustomProperties["DevTools"] as BackendExtensionModel;
            if (model == null)
                return;

            // Do something with the model now: e.g. store it ;-)
        }
    }
}