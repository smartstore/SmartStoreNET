using SmartStore.Collections;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.UI
{
    public abstract class MenuItemProviderBase : IMenuItemProvider
	{
        public IIconExplorer IconExplorer { get; set; }

        public virtual void Append(MenuItemProviderRequest request)
		{
            Guard.NotNull(request, nameof(request));
            Guard.NotNull(request.Parent, nameof(request.Parent));
			Guard.NotNull(request.Entity, nameof(request.Entity));

            // Add group header item.
            if (request.Entity.BeginGroup && !request.IsMenuEditing)
            {
                request.Parent.Append(new MenuItem
                {
                    IsGroupHeader = true,
                    Text = request.Entity.ShortDescription
                });
            }

			var node = request.Parent.Append(ConvertToMenuItem(request));
			
			ApplyLink(request, node);
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
            var shortDescription = entity.GetLocalized(x => x.ShortDescription);

			var menuItem = new MenuItem
			{
                EntityId = entity.Id,
				Text = title,
                Visible = entity.Published,
                Rtl = title?.CurrentLanguage?.Rtl ?? false,
                PermissionNames = entity.PermissionNames
            };

            if (shortDescription.HasValue())
            {
                menuItem.LinkHtmlAttributes.Add("title", shortDescription);
            }

            if (entity.NoFollow)
            {
                menuItem.LinkHtmlAttributes.Add("rel", "nofollow");
            }

            if (entity.NewWindow)
            {
                menuItem.LinkHtmlAttributes.Add("target", "_blank");
            }

            if (entity.CssClass.HasValue())
            {
                menuItem.LinkHtmlAttributes.Add("class", entity.CssClass);
            }

            if (entity.HtmlId.HasValue())
            {
                menuItem.LinkHtmlAttributes.Add("id", entity.HtmlId);
            }

            if (entity.Icon.HasValue() && !request.IsMenuEditing)
            {
                menuItem.Icon = IconExplorer.GetIconByName(entity.Icon).GetCssClass(entity.Style);
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
