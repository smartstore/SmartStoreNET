using System;
using SmartStore.Collections;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Admin.Infrastructure
{
    public partial class AdminMenu : MenuBase
    {
        public override string Name => "Admin";

        public override bool ApplyPermissions => true;

        protected override string GetCacheKey()
        {
            var cacheKey = "{0}-{1}".FormatInvariant(
                Services.WorkContext.WorkingLanguage.Id,
                Services.WorkContext.CurrentCustomer.GetRolesIdent());

            return cacheKey;
        }

        protected override TreeNode<MenuItem> Build()
        {
            var xmlSiteMap = new Telerik.Web.Mvc.XmlSiteMap();
            xmlSiteMap.LoadFrom("~/Administration/sitemap.config");

            var rootNode = ConvertSitemapNodeToMenuItemNode(xmlSiteMap.RootNode);

            Services.EventPublisher.Publish(new MenuBuiltEvent(Name, rootNode));

            return rootNode;
        }

        protected virtual TreeNode<MenuItem> ConvertSitemapNodeToMenuItemNode(Telerik.Web.Mvc.SiteMapNode node)
        {
            var item = new MenuItem();
            var root = new TreeNode<MenuItem>(item);

            var id = node.Attributes.ContainsKey("id")
                ? node.Attributes["id"] as string
                : string.Empty;

            root.Id = id;

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

            item.Id = id;
            item.Visible = node.Visible;
            item.Text = node.Title;
            item.Attributes.Merge(node.Attributes);

            if (node.Attributes.ContainsKey("permissionNames"))
            {
                item.PermissionNames = node.Attributes["permissionNames"] as string;
            }

            if (node.Attributes.ContainsKey("resKey"))
            {
                item.ResKey = node.Attributes["resKey"] as string;
            }

            if (node.Attributes.ContainsKey("iconClass"))
            {
                item.Icon = node.Attributes["iconClass"] as string;
            }

            if (node.Attributes.ContainsKey("imageUrl"))
            {
                item.ImageUrl = node.Attributes["imageUrl"] as string;
            }

            if (node.Attributes.ContainsKey("isGroupHeader"))
            {
                item.IsGroupHeader = Boolean.Parse(node.Attributes["isGroupHeader"] as string);
            }

            // Iterate children recursively.
            foreach (var childNode in node.ChildNodes)
            {
                var childTreeNode = ConvertSitemapNodeToMenuItemNode(childNode);
                root.Append(childTreeNode);
            }

            return root;
        }
    }
}