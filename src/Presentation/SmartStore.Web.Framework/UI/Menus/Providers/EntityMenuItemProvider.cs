using SmartStore.Collections;
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

        protected override void ApplyLink(MenuItemProviderRequest request, TreeNode<MenuItem> node)
		{
            if (request.Entity.Model.IsEmpty())
            {
                return;
            }

            // Always resolve against current store, current customer and working language.
            var result = _linkResolver.Resolve(request.Entity.Model);
			node.Value.Url = result.Link;

            if (node.Value.Text.IsEmpty())
            {
                node.Value.Text = result.Label;
            }

            // Only apply MenuItemRecord.Published for edit mode.
            if (!request.Origin.IsCaseInsensitiveEqual("EditMenu"))
            {
                node.Value.Visible = result.Status == LinkStatus.Ok;
            }
        }
    }
}
