using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Plugins;

namespace SmartStore.Web.Framework.UI
{
	// INFO: The provider's SystemName is also the edit template name > Views/Shared/EditorTemplates/MenuItem.{SystemName}.cshtml.
	// Model is: string
	[SystemName("entity")]
	public class EntityMenuItemProvider : MenuItemProviderBase
	{
		protected override void ApplyLink(TreeNode<MenuItem> node, MenuItemRecord entity)
		{
			var url = string.Empty; // TODO: create url with LinkResolver
			var entityName = string.Empty; // TODO: determine entity name/type, if applicable
			var id = 0; // TODO: determine entity id, if applicable

			node.Id = entity + "." + id;
			node.Value.EntityId = id;
			node.Value.Url = url;

			// TBD: What about cache invalidation? We would also have two levels of caching:
			// One in the LinkResolver and one in the menu system.
		}
	}
}
