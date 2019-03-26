using SmartStore.Collections;
using SmartStore.Core.Domain.Cms;

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
			var menuItem = new MenuItem
			{
				Text = entity.Title,
                Visible = entity.Published
			};

            // TODO: support divider elements.
            //menuItem.Attributes.Add("IsDivider", entity.IsDivider);

            if (entity.NoFollow)
            {
                menuItem.LinkHtmlAttributes.Add("rel", "nofollow");
            }
            if (entity.ShortDescription.HasValue())
            {
                menuItem.LinkHtmlAttributes.Add("title", entity.ShortDescription);
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
