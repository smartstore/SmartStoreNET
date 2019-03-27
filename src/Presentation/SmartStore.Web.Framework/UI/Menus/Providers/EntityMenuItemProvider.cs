using SmartStore.Collections;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Plugins;
using SmartStore.Services.Cms;

namespace SmartStore.Web.Framework.UI
{
    // INFO: The provider's SystemName is also the edit template name > Views/Shared/EditorTemplates/MenuItem.{SystemName}.cshtml.
    // Model is: string
    [SystemName("entity")]
	public class EntityMenuItemProvider : MenuItemProviderBase
	{
        private readonly ILinkResolver _linkResolver;

        public EntityMenuItemProvider(ILinkResolver linkResolver)
        {
            _linkResolver = linkResolver;
        }

        protected override void ApplyLink(TreeNode<MenuItem> node, MenuItemRecord entity)
		{
            if (entity.Model.IsEmpty())
            {
                return;
            }

            // TBD: to resolve for what roles, what language, what store?
            var result = _linkResolver.Resolve(entity.Model);

            node.Id = string.Concat(result.Type.ToString().ToLower(), ".", result.Id);
			node.Value.EntityId = result.Id;
			node.Value.Url = result.Link;

			// TBD: What about cache invalidation? We would also have two levels of caching:
			// One in the LinkResolver and one in the menu system.
		}
	}
}
