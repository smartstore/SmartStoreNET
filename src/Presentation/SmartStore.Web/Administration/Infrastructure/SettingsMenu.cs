using System.Web.Routing;
using SmartStore.Collections;
using SmartStore.Core.Localization;
using SmartStore.Core.Security;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Admin.Infrastructure
{
    public class SettingsMenu : MenuBase
    {
        public Localizer T { get; set; } = NullLocalizer.Instance;

        public override string Name => "Settings";

        protected override string GetCacheKey()
        {
            var cacheKey = "{0}-{1}".FormatInvariant(
                Services.WorkContext.WorkingLanguage.Id,
                Services.WorkContext.CurrentCustomer.GetRolesIdent());

            return cacheKey;
        }

        protected override TreeNode<MenuItem> Build()
        {
            var store = Services.StoreContext.CurrentStore;
            var customer = Services.WorkContext.CurrentCustomer;
            var perm = Permissions.Configuration.Setting.Read;

            var root = new TreeNode<MenuItem>(new MenuItem { Text = T("Admin.Configuration.Settings") })
            {
                Id = Name
            };

            root.Append(new MenuItem
            {
                Id = "general",
                Text = T("Admin.Common.General"),
                Icon = "fa fa-fw fa-sliders-h",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "GeneralCommon"
            });

            root.Append(new MenuItem
            {
                Id = "catalog",
                Text = T("Admin.Catalog"),
                Icon = "fas fa-fw fa-book",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "Catalog"
            });

            root.Append(new MenuItem
            {
                Id = "search",
                Text = T("Search.Title"),
                Icon = "far fa-fw fa-search",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "Search"
            });

            root.Append(new MenuItem
            {
                Id = "customer",
                Text = T("Admin.Customers"),
                Icon = "fa fa-fw fa-users",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "CustomerUser"
            });

            root.Append(new MenuItem
            {
                Id = "cart",
                Text = T("ShoppingCart"),
                Icon = "fa fa-fw fa-shopping-cart",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "ShoppingCart"
            });

            root.Append(new MenuItem
            {
                Id = "order",
                Text = T("Admin.Orders"),
                Icon = "fa fa-fw fa-chart-bar",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "Order"
            });

            root.Append(new MenuItem
            {
                Id = "payment",
                Text = T("Admin.Configuration.Payment"),
                Icon = "far fa-fw fa-credit-card",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "Payment"
            });

            root.Append(new MenuItem
            {
                Id = "tax",
                Text = T("Admin.Plugins.KnownGroup.Tax"),
                Icon = "fa fa-fw fa-percent",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "Tax"
            });

            root.Append(new MenuItem
            {
                Id = "shipping",
                Text = T("Admin.Configuration.Shipping"),
                Icon = "fa fa-fw fa-truck",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "Shipping"
            });

            root.Append(new MenuItem
            {
                Id = "reward-points",
                Text = T("Account.RewardPoints"),
                Icon = "fa fa-fw fa-trophy",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "RewardPoints"
            });

            root.Append(new MenuItem
            {
                Id = "media",
                Text = T("Admin.Plugins.KnownGroup.Media"),
                Icon = "far fa-fw fa-image",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "Media"
            });

            root.Append(new MenuItem
            {
                Id = "blog",
                Text = T("Blog"),
                Icon = "far fa-fw fa-edit",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "Blog"
            });

            root.Append(new MenuItem
            {
                Id = "news",
                Text = T("News"),
                Icon = "far fa-fw fa-rss",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "News"
            });

            root.Append(new MenuItem
            {
                Id = "forum",
                Text = T("Forum.Forums"),
                Icon = "fa fa-fw fa-users",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "Forum"
            });

            root.Append(new MenuItem
            {
                Id = "dataexchange",
                Text = T("Admin.Common.DataExchange"),
                Icon = "fa fa-fw fa-exchange-alt",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "DataExchange"
            });

            root.Append(new MenuItem
            {
                IsGroupHeader = true,
                Id = "all",
                Text = T("Admin.Configuration.Settings.AllSettings"),
                Icon = "fa fa-fw fa-cogs",
                PermissionNames = perm,
                RouteValues = new RouteValueDictionary { ["area"] = "admin" },
                ControllerName = "Setting",
                ActionName = "AllSettings"
            });

            return root;
        }
    }
}