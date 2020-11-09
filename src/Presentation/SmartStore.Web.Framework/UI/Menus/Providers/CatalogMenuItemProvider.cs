using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Localization;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Cms;
using SmartStore.Services.Localization;
using SmartStore.Services.Seo;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI
{
    [MenuItemProvider("catalog", AppendsMultipleItems = true)]
    public class CatalogMenuItemProvider : MenuItemProviderBase
    {
        private readonly ICommonServices _services;
        private readonly ICategoryService _categoryService;
        private readonly ILinkResolver _linkResolver;

        public CatalogMenuItemProvider(
            ICommonServices services,
            ICategoryService categoryService,
            ILinkResolver linkResolver)
        {
            _services = services;
            _categoryService = categoryService;
            _linkResolver = linkResolver;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override TreeNode<MenuItem> Append(MenuItemProviderRequest request)
        {
            if (request.IsEditMode)
            {
                var item = ConvertToMenuItem(request);
                item.Summary = T("Providers.MenuItems.FriendlyName.Catalog");
                item.Icon = "fa fa-cubes";

                AppendToParent(request, item);
            }
            else
            {
                var tree = _categoryService.GetCategoryTree(0, false, _services.StoreContext.CurrentStore.Id);
                var randomId = CommonHelper.GenerateRandomInteger(0, 1000000);

                if (request.Entity.BeginGroup)
                {
                    AppendToParent(request, new MenuItem
                    {
                        IsGroupHeader = true,
                        Text = request.Entity.GetLocalized(x => x.ShortDescription)
                    });
                }

                // Do not append the root itself.
                foreach (var child in tree.Children)
                {
                    AppendToParent(request, ConvertNode(request, child, ref randomId));
                }
            }

            // Do not traverse appended items.
            return null;

            // TBD: Cache invalidation workflow changes, because the catalog tree 
            // is now contained within other menus. Invalidating the tree now means:
            // invalidate all containing menus also.
        }

        protected override void ApplyLink(MenuItemProviderRequest request, TreeNode<MenuItem> node)
        {
            // Void, does nothing here.
        }

        private TreeNode<MenuItem> ConvertNode(
            MenuItemProviderRequest request,
            TreeNode<ICategoryNode> categoryNode,
            ref int randomId)
        {
            var node = categoryNode.Value;
            var name = node.Id > 0 ? node.GetLocalized(x => x.Name) : null;

            var menuItem = new MenuItem
            {
                Id = randomId++.ToString(),
                EntityId = node.Id,
                EntityName = nameof(Category),
                MenuItemId = request.Entity.Id,
                Text = name?.Value ?? node.Name,
                Rtl = name?.CurrentLanguage?.Rtl ?? false,
                BadgeText = node.Id > 0 ? node.GetLocalized(x => x.BadgeText) : null,
                BadgeStyle = (BadgeStyle)node.BadgeStyle,
                RouteName = node.Id > 0 ? "Category" : "HomePage",
                ImageId = node.MediaFileId
            };

            // Handle external link
            if (node.ExternalLink.HasValue())
            {
                var link = _linkResolver.Resolve(node.ExternalLink);
                if (link.Status == LinkStatus.Ok)
                {
                    menuItem.Url = link.Link;
                }
            }

            if (menuItem.Url.IsEmpty())
            {
                if (node.Id > 0)
                {
                    menuItem.RouteName = "Category";
                    menuItem.RouteValues.Add("SeName", node.GetSeName());
                }
                else
                {
                    menuItem.RouteName = "HomePage";
                }
            }

            // Picture
            if (node.Id > 0 && node.ParentCategoryId == 0 && node.Published && node.MediaFileId != null)
            {
                menuItem.ImageId = node.MediaFileId;
            }

            // Apply inheritable properties.
            menuItem.Visible = request.Entity.Published;
            menuItem.PermissionNames = request.Entity.PermissionNames;

            if (request.Entity.NoFollow)
            {
                menuItem.LinkHtmlAttributes.Add("rel", "nofollow");
            }

            if (request.Entity.NewWindow)
            {
                menuItem.LinkHtmlAttributes.Add("target", "_blank");
            }

            var convertedNode = new TreeNode<MenuItem>(menuItem)
            {
                Id = categoryNode.Id
            };

            if (categoryNode.HasChildren)
            {
                foreach (var childNode in categoryNode.Children)
                {
                    convertedNode.Append(ConvertNode(request, childNode, ref randomId));
                }
            }

            return convertedNode;
        }
    }
}
