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
    public class SiteMapBuiltEventConsumer : IConsumer<SiteMapBuiltEvent>
    {
        private readonly ICommonServices _services;
        private readonly CatalogSettings _catalogSettings;

        public SiteMapBuiltEventConsumer(ICommonServices services, CatalogSettings catalogSettings)
        {
            _services = services;
            _catalogSettings = catalogSettings;
        }

        public void HandleEvent(SiteMapBuiltEvent eventMessage)
        {
            if (eventMessage.Name.Equals("catalog") && _catalogSettings.MaxItemsToDisplayInCatalogMenu != null)
            {
                var navigationModel = eventMessage.Root;
                var cutOffItems = new List<TreeNode<MenuItem>>();
                var newNavMmenuItem = new TreeNode<MenuItem>(new MenuItem());
                newNavMmenuItem.Value.Text = _services.Localization.GetResource("CatalogMenu.MoreLink");
                newNavMmenuItem.Value.Id = "MoreItem";
                newNavMmenuItem.Value.EntityId = -1;
                newNavMmenuItem.Value.ActionName = "Index";
                newNavMmenuItem.Value.ControllerName = "Home";

                cutOffItems = navigationModel.Root.Children
                    // TODO: next statement would be much better code but can't be used because Id ist null for nearly all treenodes
                    //.Where(x => !x.Value.Id.Equals("manufacturer"))
                    .Where(x => x.Value.Id == null)
                    .Skip((int)_catalogSettings.MaxItemsToDisplayInCatalogMenu)
                    .ToList();

                newNavMmenuItem.AppendRange(cutOffItems);

                navigationModel.Root.Append(newNavMmenuItem);
            }
        }
    }
}
