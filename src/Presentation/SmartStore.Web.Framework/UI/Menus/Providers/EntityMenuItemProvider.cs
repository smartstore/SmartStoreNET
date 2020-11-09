using SmartStore.Collections;
using SmartStore.Core.Localization;
using SmartStore.Services.Cms;

namespace SmartStore.Web.Framework.UI
{
    // INFO: The provider's SystemName is also the edit template name > Views/Shared/EditorTemplates/MenuItem.{SystemName}.cshtml.
    // Model is: string
    [MenuItemProvider("entity")]
    public class EntityMenuItemProvider : MenuItemProviderBase
    {
        private readonly ILinkResolver _linkResolver;

        public EntityMenuItemProvider(ILinkResolver linkResolver)
        {
            _linkResolver = linkResolver;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override void ApplyLink(MenuItemProviderRequest request, TreeNode<MenuItem> node)
        {
            // Always resolve against current store, current customer and working language.
            var result = _linkResolver.Resolve(request.Entity.Model);
            var item = node.Value;

            item.Url = result.Link;
            item.ImageId = result.PictureId;

            if (item.Text.IsEmpty())
            {
                item.Text = result.Label;
            }

            switch (result.Type)
            {
                case LinkType.Product:
                case LinkType.Category:
                case LinkType.Manufacturer:
                case LinkType.Topic:
                    if (request.IsEditMode)
                    {
                        // Info: node.Value.EntityId is MenuItemRecord.Id for editing MenuItemRecord.
                    }
                    else
                    {
                        item.EntityId = result.Id;
                        item.EntityName = result.Type.ToString();
                    }
                    break;
            }

            if (request.IsEditMode)
            {
                var info = result.Type.GetLinkTypeInfo();
                item.Summary = T(info.ResKey);
                item.Icon = info.Icon;

                if (item.Url.IsEmpty())
                {
                    item.Text = null;
                    item.ResKey = "Admin.ContentManagement.Menus.SpecifyLinkTarget";
                }
            }
            else
            {
                // For edit mode, only apply MenuItemRecord.Published.
                item.Visible = result.Status == LinkStatus.Ok;
            }
        }
    }
}
