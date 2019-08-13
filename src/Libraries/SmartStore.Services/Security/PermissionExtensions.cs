using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Security;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Security
{
    public static class PermissionExtensions
    {
        public static void GetDisplayNames(this TreeNode<IPermissionNode> tree, ILocalizationService localizationService)
        {
            Guard.NotNull(localizationService, nameof(localizationService));

            if (tree == null)
            {
                return;
            }

            var tokenSeparator = new char[] { '.' };
            var names = new Dictionary<string, string>();

            Add("read", "Common.Read");
            Add("update", "Common.Edit");
            Add("create", "Common.Create");
            Add("delete", "Common.Delete");
            Add("catalog", "Admin.Catalog");

            foreach (PermissionNode node in tree.Flatten())
            {
                var tokens = node.SystemName.EmptyNull().ToLower().Split(tokenSeparator, StringSplitOptions.RemoveEmptyEntries);
                var token = tokens.LastOrDefault();
                if (token != null)
                {
                    // Known token of default permissions.
                    if (!names.TryGetValue(token, out var name))
                    {
                        // Unknown token (e.g. permission added by plugin). Try resource by name convention.
                        var key = "Permissions.DisplayName." + token.Replace("-", "");
                        name = localizationService.GetResource(key, defaultValue: token);
                    }

                    node.DisplayName = string.IsNullOrWhiteSpace(name) ? token : name;
                }
                else
                {
                    node.DisplayName = node.SystemName;
                }
            }

            void Add(string token, string resourceKey)
            {
                names.Add(token, localizationService.GetResource(resourceKey, defaultValue: token));
            }
        }
    }
}
