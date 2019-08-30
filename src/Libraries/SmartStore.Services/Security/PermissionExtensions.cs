using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Services.Security
{
    public static class PermissionExtensions
    {
        /// <summary>
        /// Set all node display names for <paramref name="tree"/>.
        /// For new string resources the resource key schema "Permissions.DisplayName.[token]" is to be used.
        /// </summary>
        /// <param name="tree">Permission tree.</param>
        /// <param name="localizationService">Localization service.</param>
        public static void GetDisplayNames(this TreeNode<IPermissionNode> tree, ICommonServices services)
        {
            Guard.NotNull(services, nameof(services));

            if (tree == null)
            {
                return;
            }

            var resourceKeys = new Dictionary<string, string>
            {
                { "read", "Common.Read" },
                { "update", "Common.Edit" },
                { "create", "Common.Create" },
                { "delete", "Common.Delete" },
                { "catalog", "Admin.Catalog" },
                { "product", "Admin.Catalog.Products" },
                { "category", "Admin.Catalog.Categories" },
                { "manufacturer", "Admin.Catalog.Manufacturers" },
                { "variant", "Admin.Catalog.Attributes.ProductAttributes" },
                { "attribute", "Admin.Catalog.Attributes.SpecificationAttributes" },
                { "customer", "Admin.Customers" },
                { "impersonate", "Admin.Customers.Customers.Impersonate" },
                { "role", "Admin.Customers.CustomerRoles" },
                { "order", "Admin.Orders" },
                { "gift-card", "Admin.GiftCards" },
                { "notify", "Common.Notify" },
                { "return-request", "Admin.ReturnRequests" },
                { "accept", "Admin.ReturnRequests.Accept" },
                { "promotion", "Admin.Catalog.Products.Promotion" },
                { "affiliate", "Admin.Affiliates" },
                { "campaign", "Admin.Promotions.Campaigns" },
                { "discount", "Admin.Promotions.Discounts" },
                { "newsletter", "Admin.Promotions.NewsLetterSubscriptions" },
                { "cms", "Admin.ContentManagement" },
                { "poll", "Admin.ContentManagement.Polls" },
                { "news", "Admin.ContentManagement.News" },
                { "blog", "Admin.ContentManagement.Blog" },
                { "widget", "Admin.ContentManagement.Widgets" },
                { "topic", "Admin.ContentManagement.Topics" },
                { "menu", "Admin.ContentManagement.Menus" },
                { "forum", "Admin.ContentManagement.Forums" },
                { "message-template", "Admin.ContentManagement.MessageTemplates" },
                { "configuration", "Admin.Configuration" },
                { "country", "Admin.Configuration.Countries" },
                { "language", "Admin.Configuration.Languages" },
                { "setting", "Admin.Configuration.Settings" },
                { "payment-method", "Admin.Configuration.Payment.Methods" },
                { "activate", "Admin.Common.Activate" },
                { "authentication", "Admin.Configuration.ExternalAuthenticationMethods" },
                { "currency", "Admin.Configuration.Currencies" },
                { "delivery-time", "Admin.Configuration.DeliveryTimes" },
                { "theme", "Admin.Configuration.Themes" },
                { "measure", "Admin.Configuration.Measures.Dimensions" },
                { "activity-log", "Admin.Configuration.ActivityLog.ActivityLogType" },
                { "acl", "Admin.Configuration.ACL" },
                { "email-account", "Admin.Configuration.EmailAccounts" },
                { "store", "Admin.Common.Stores" },
                { "shipping", "Admin.Configuration.DeliveryTimes" },
                { "tax", "Admin.Configuration.Tax.Providers" },
                { "plugin", "Admin.Configuration.Plugins" },
                { "upload", "Common.Upload" },
                { "install", "Admin.Configuration.Plugins.Fields.Install" },
                { "license", "Admin.Common.License" },
                { "export", "Common.Export" },
                { "execute", "Admin.Common.Go" },
                { "import", "Common.Import" },
                { "system", "Admin.System" },
                { "administrate", "Admin.Plugins.KnownGroup.Admin" },
                { "log", "Admin.System.Log" },
                { "message", "Admin.System.QueuedEmails" },
                { "send", "Common.Send" },
                { "maintenance", "Admin.System.Maintenance" },
                { "schedule-task", "Admin.System.ScheduleTasks" },
                { "url-record", "Admin.System.SeNames" },
                { "cart", "ShoppingCart" },
                { "checkout-attribute", "Admin.Catalog.Attributes.CheckoutAttributes" },
                { "media", "Admin.Plugins.KnownGroup.Media" }
            };

            var tokenSeparator = new char[] { '.' };
            var language = services.WorkContext.WorkingLanguage;
            var allKeys = resourceKeys.Select(x => x.Value);

            var resourceQuery = services.Localization.All(language.Id);
            var resources = resourceQuery.Where(x => allKeys.Contains(x.ResourceName))
                .ToList()
                .ToDictionarySafe(x => x.ResourceName, x => x.ResourceValue);

            AddDisplayName(tree);

            void AddDisplayName(TreeNode<IPermissionNode> node)
            {
                var tokens = node.Value.SystemName.EmptyNull().ToLower().Split(tokenSeparator, StringSplitOptions.RemoveEmptyEntries);
                var token = tokens.LastOrDefault();
                if (token != null)
                {
                    // Try known token of default permissions.
                    if (!resourceKeys.TryGetValue(token, out var key) || !resources.TryGetValue(key, out var name))
                    {
                        // Unknown token. Try to find resource by name convention.
                        key = "Permissions.DisplayName." + token.Replace("-", "");

                        // Try resource provided by core.
                        name = services.Localization.GetResource(key, language.Id, false, string.Empty, true);
                        if (name.IsEmpty())
                        {
                            // Try resource provided by plugin.
                            name = services.Localization.GetResource("Plugins." + key, language.Id, false, string.Empty, true);
                        }
                    }

                    node.SetThreadMetadata("DisplayName", string.IsNullOrWhiteSpace(name) ? token : name);
                }
                else
                {
                    node.SetThreadMetadata("DisplayName", node.Value.SystemName);
                }
                
                if (node.HasChildren)
                {
                    node.Children.Each(x => AddDisplayName(x));
                }
            }
        }
    }
}
