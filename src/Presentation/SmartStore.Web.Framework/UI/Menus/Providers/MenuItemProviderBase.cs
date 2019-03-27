using SmartStore.Collections;
using SmartStore.Core.Domain.Cms;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.UI
{
    public abstract class MenuItemProviderBase : IMenuItemProvider
	{
		public virtual void Append(TreeNode<MenuItem> parent, MenuItemRecord entity)
		{
			Guard.NotNull(parent, nameof(parent));
			Guard.NotNull(entity, nameof(entity));

			var menuItem = parent.Append(ConvertToMenuItem(entity));
			
			ApplyLink(menuItem, entity);
		}

		/// <summary>
		/// Converts the passed menu item entity to a <see cref="MenuItem"/> object.
		/// </summary>
		/// <param name="entity">The entity to convert.</param>
		/// <returns>Menu item.</returns>
		protected virtual MenuItem ConvertToMenuItem(MenuItemRecord entity)
		{
            var shortDescription = entity.GetLocalized(x => x.ShortDescription);

			var menuItem = new MenuItem
			{
                EntityId = entity.Id,
				Text = entity.GetLocalized(x => x.Title),
                Visible = entity.Published
			};

            if (entity.NoFollow)
            {
                menuItem.LinkHtmlAttributes.Add("rel", "nofollow");
            }
            if (shortDescription.HasValue())
            {
                menuItem.LinkHtmlAttributes.Add("title", shortDescription);
            }
            if (entity.NewWindow)
            {
                menuItem.LinkHtmlAttributes.Add("target", "_blank");
            }
            if (entity.HtmlId.HasValue())
            {
                menuItem.LinkHtmlAttributes.Add("id", entity.HtmlId);
            }
            if (entity.CssClass.HasValue())
            {
                menuItem.LinkHtmlAttributes.Add("class", entity.CssClass);
            }

            // TODO: entity.ShowExpanded

            return menuItem;
		}

		/// <summary>
		/// Generates and applies the link to the converted <see cref="MenuItem"/> object.
		/// </summary>
		/// <param name="node">The newly created menu item node to apply the generated link to.</param>
		/// <param name="entity">The entity contains information about the type of link.</param>
		protected abstract void ApplyLink(TreeNode<MenuItem> node, MenuItemRecord entity);
	}
}
