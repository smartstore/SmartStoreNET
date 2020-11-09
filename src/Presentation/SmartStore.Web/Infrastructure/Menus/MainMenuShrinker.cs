using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;
using SmartStore.Services;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Infrastructure
{
    public class MainMenuShrinker : IConsumer
    {
        private readonly ICommonServices _services;
        private readonly CatalogSettings _catalogSettings;

        public MainMenuShrinker(ICommonServices services, CatalogSettings catalogSettings)
        {
            _services = services;
            _catalogSettings = catalogSettings;
        }

        public void HandleEvent(MenuBuiltEvent eventMessage)
        {
            if (eventMessage.Name != "Main" || _catalogSettings.MaxItemsToDisplayInCatalogMenu.GetValueOrDefault() < 1)
            {
                return;
            }

            eventMessage.Root.Children
                .Where(x => x.Value.Id != "brand")
                .Skip(_catalogSettings.MaxItemsToDisplayInCatalogMenu.Value)
                .Each(x => x.SetMetadata("spare", true));
        }
    }
}
