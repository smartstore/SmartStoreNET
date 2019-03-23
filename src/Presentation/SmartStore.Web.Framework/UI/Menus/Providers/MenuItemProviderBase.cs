using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		/// Converts the passed menu item entity to a <see cref="MenuItem"/> object
		/// </summary>
		/// <param name="entity">The entity to convert.</param>
		/// <returns></returns>
		protected virtual MenuItem ConvertToMenuItem(MenuItemRecord entity)
		{
			var menuItem = new MenuItem
			{
				// TODO: convert
			};

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
