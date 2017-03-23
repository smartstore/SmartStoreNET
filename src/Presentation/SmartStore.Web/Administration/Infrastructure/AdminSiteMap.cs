using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Search;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Admin.Infrastructure
{
	public class AdminSiteMap : SiteMapBase
	{
		private static object s_lock = new object();

		private readonly ICategoryService _categoryService;
		private readonly IPictureService _pictureService;
		private readonly CatalogSettings _catalogSettings;
		private readonly ICatalogSearchService _catalogSearchService;

		public AdminSiteMap(
			ICategoryService categoryService,
			IPictureService pictureService,
			CatalogSettings catalogSettings,
			ICatalogSearchService catalogSearchService)
		{
			_categoryService = categoryService;
			_pictureService = pictureService;
			_catalogSettings = catalogSettings;
			_catalogSearchService = catalogSearchService;

			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		public override string Name
		{
			get { return "admin"; }
		}

		public override bool ApplyPermissions
		{
			get { return true; }
		}

		protected override string GetCacheKey()
		{
			var customerRolesIds = Services.WorkContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();
			string cacheKey = "{0}-{1}".FormatInvariant(
				Services.WorkContext.WorkingLanguage.Id,
				string.Join(",", customerRolesIds));

			return cacheKey;
		}

		protected override TreeNode<MenuItem> Build()
		{
			var xmlSiteMap = new Telerik.Web.Mvc.XmlSiteMap();
			xmlSiteMap.LoadFrom("~/Administration/sitemap.config");

			var rootNode = ConvertSitemapNodeToMenuItemNode(xmlSiteMap.RootNode);

			return rootNode;
		}

		private TreeNode<MenuItem> ConvertSitemapNodeToMenuItemNode(Telerik.Web.Mvc.SiteMapNode node)
		{
			var item = new MenuItem();
			var treeNode = new TreeNode<MenuItem>(item);

			if (node.RouteName.HasValue())
			{
				item.RouteName = node.RouteName;
			}
			else if (node.ActionName.HasValue() && node.ControllerName.HasValue())
			{
				item.ActionName = node.ActionName;
				item.ControllerName = node.ControllerName;
			}
			else if (node.Url.HasValue())
			{
				item.Url = node.Url;
			}
			item.RouteValues = node.RouteValues;

			item.Visible = node.Visible;
			item.Text = node.Title;
			item.Attributes.Merge(node.Attributes);

			if (node.Attributes.ContainsKey("permissionNames"))
				item.PermissionNames = node.Attributes["permissionNames"] as string;

			if (node.Attributes.ContainsKey("id"))
				item.Id = node.Attributes["id"] as string;

			if (node.Attributes.ContainsKey("resKey"))
				item.ResKey = node.Attributes["resKey"] as string;

			if (node.Attributes.ContainsKey("iconClass"))
				item.Icon = node.Attributes["iconClass"] as string;

			if (node.Attributes.ContainsKey("imageUrl"))
				item.ImageUrl = node.Attributes["imageUrl"] as string;

			if (node.Attributes.ContainsKey("isGroupHeader"))
				item.IsGroupHeader = Boolean.Parse(node.Attributes["isGroupHeader"] as string);

			// iterate children recursively
			foreach (var childNode in node.ChildNodes)
			{
				var childTreeNode = ConvertSitemapNodeToMenuItemNode(childNode);
				treeNode.Append(childTreeNode);
			}

			return treeNode;
		}
	}
}