using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.UI
{
    public abstract class MenuItemProviderBase : IMenuItemProvider
    {
        public IIconExplorer IconExplorer { get; set; }

        public virtual TreeNode<MenuItem> Append(MenuItemProviderRequest request)
        {
            Guard.NotNull(request, nameof(request));
            Guard.NotNull(request.Parent, nameof(request.Parent));
            Guard.NotNull(request.Entity, nameof(request.Entity));

            // Add group header item.
            if (request.Entity.BeginGroup && !request.IsEditMode)
            {
                AppendToParent(request, new MenuItem
                {
                    IsGroupHeader = true,
                    Text = request.Entity.GetLocalized(x => x.ShortDescription)
                });
            }

            var node = AppendToParent(request, ConvertToMenuItem(request));

            ApplyLink(request, node);

            return node;
        }

        protected virtual TreeNode<MenuItem> AppendToParent(MenuItemProviderRequest request, MenuItem item)
        {
            return AppendToParent(request, new TreeNode<MenuItem>(item));
        }

        protected virtual TreeNode<MenuItem> AppendToParent(MenuItemProviderRequest request, TreeNode<MenuItem> node)
        {
            var root = request.Parent.Root;
            var providers = root.GetMetadata<List<string>>("Providers");
            var provider = request.Entity.ProviderName;

            node.SetMetadata("Provider", provider);

            if (providers == null)
            {
                root.SetMetadata("Providers", new List<string> { provider });
            }
            else if (!providers.Contains(provider))
            {
                providers.Add(provider);
            }

            if (node.Id == null && node.Value.Id.HasValue())
            {
                node.Id = node.Value.Id;
            }

            return request.Parent.Append(node);
        }

        /// <summary>
        /// Converts the passed menu item entity to a <see cref="MenuItem"/> object.
        /// </summary>
        /// <param name="entity">The entity to convert.</param>
        /// <returns>Menu item.</returns>
        protected virtual MenuItem ConvertToMenuItem(MenuItemProviderRequest request)
        {
            var entity = request.Entity;
            var title = entity.GetLocalized(x => x.Title);
            string shortDescription = entity.GetLocalized(x => x.ShortDescription);

            var menuItem = new MenuItem
            {
                EntityId = entity.Id,
                EntityName = "MenuItem", // nameof(MenuItemRecord),
                MenuItemId = entity.Id,
                Text = title,
                Summary = shortDescription,
                Visible = entity.Published,
                Rtl = title?.CurrentLanguage?.Rtl ?? false,
                PermissionNames = entity.PermissionNames
            };

            // Common attributes

            if (shortDescription.HasValue())
            {
                menuItem.HtmlAttributes.Add("title", shortDescription);
            }

            if (entity.CssClass.HasValue())
            {
                menuItem.HtmlAttributes.Add("class", entity.CssClass);
            }

            if (entity.HtmlId.HasValue())
            {
                menuItem.HtmlAttributes.Add("id", entity.HtmlId);
                menuItem.Id = entity.HtmlId;
            }

            // Link attributes

            if (entity.NoFollow)
            {
                menuItem.LinkHtmlAttributes.Add("rel", "nofollow");
            }

            if (entity.NewWindow)
            {
                menuItem.LinkHtmlAttributes.Add("target", "_blank");
            }

            // Icon

            if (entity.Icon.HasValue() && !request.IsEditMode)
            {
                menuItem.Icon = IconExplorer.GetIconByName(entity.Icon).GetCssClass(entity.Style);

                if (entity.IconColor.HasValue())
                {
                    menuItem.Icon += " text-" + entity.IconColor;
                }
            }

            // For future use: entity.ShowExpanded

            return menuItem;
        }

        /// <summary>
        /// Generates and applies the link to the converted <see cref="MenuItem"/> object.
        /// </summary>
        /// <param name="request">Contains information about the request to the provider.</param>
        /// <param name="node">The newly created menu item node to apply the generated link to.</param>
        protected abstract void ApplyLink(MenuItemProviderRequest request, TreeNode<MenuItem> node);
    }
}
