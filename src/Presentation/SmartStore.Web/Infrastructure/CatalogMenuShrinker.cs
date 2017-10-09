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

            var root = eventMessage.Root;
            var newNavMmenuItem = new TreeNode<MenuItem>(new MenuItem
			{
				Id = "MoreItem",
				EntityId = -1,
				Text = _services.Localization.GetResource("CatalogMenu.MoreLink"),
				Url = "#"
			});

			var cutOffItems = root.Children
				// TODO: next statement would be much better code but can't be used because Id ist null for nearly all treenodes
				//.Where(x => !x.Value.Id.Equals("manufacturer"))
				.Where(x => x.Value.Id == null)
				.Skip(_catalogSettings.MaxItemsToDisplayInCatalogMenu.Value)
                .ToList();

            newNavMmenuItem.AppendRange(cutOffItems);

            root.Append(newNavMmenuItem);
        }
    }
}
