using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;
using SmartStore.Services;
using SmartStore.Web.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.Web.Infrastructure
{
    public class CatalogMenuShrinker : IConsumer<SiteMapBuiltEvent>
    {
        private readonly ICommonServices _services;
        private readonly CatalogSettings _catalogSettings;

        public CatalogMenuShrinker(ICommonServices services, CatalogSettings catalogSettings)
        {
            _services = services;
            _catalogSettings = catalogSettings;
        }

        public void HandleEvent(SiteMapBuiltEvent eventMessage)
        {
            if (eventMessage.Name != "catalog" || _catalogSettings.MaxItemsToDisplayInCatalogMenu.GetValueOrDefault() < 1)
                return;
            
            eventMessage.Root.Children
                .Where(x => x.Value.Id == null)
                .Skip(_catalogSettings.MaxItemsToDisplayInCatalogMenu.Value)
                .Each(x => x.SetMetadata("spare", true) );
        }
    }
}
