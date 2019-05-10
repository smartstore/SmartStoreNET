using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Localization;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;

namespace SmartStore.Web.Framework.UI
{
    [MenuItemProvider("catalog", AppendsMultipleItems = true)]
	public class CatalogMenuItemProvider : MenuItemProviderBase
	{
        private readonly ICommonServices _services;
        private readonly ICategoryService _categoryService;
        private readonly IPictureService _pictureService;

        public CatalogMenuItemProvider(
            ICommonServices services,
            ICategoryService categoryService,
            IPictureService pictureService)
        {
            _services = services;
            _categoryService = categoryService;
            _pictureService = pictureService;

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
                var allPictureIds = tree.Flatten().Select(x => x.PictureId.GetValueOrDefault());
                var allPictureInfos = _pictureService.GetPictureInfos(allPictureIds);

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
                    AppendToParent(request, ConvertNode(request, child, allPictureInfos));
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
            IDictionary<int, PictureInfo> allPictureInfos)
        {
            var node = categoryNode.Value;
            var name = node.Id > 0 ? node.GetLocalized(x => x.Name) : null;

            var menuItem = new MenuItem
            {
                EntityId = node.Id,
                EntityName = nameof(Category),
                Text = name?.Value ?? node.Name,
                Rtl = name?.CurrentLanguage?.Rtl ?? false,
                BadgeText = node.Id > 0 ? node.GetLocalized(x => x.BadgeText) : null,
                BadgeStyle = (BadgeStyle)node.BadgeStyle,
                RouteName = node.Id > 0 ? "Category" : "HomePage"
            };

            if (node.Id > 0)
            {
                menuItem.RouteValues.Add("SeName", node.GetSeName());

                if (node.ParentCategoryId == 0 && node.Published && node.PictureId != null)
                {
                    menuItem.ImageId = node.PictureId;
                }
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
                    convertedNode.Append(ConvertNode(request, childNode, allPictureInfos));
                }
            }

            return convertedNode;
        }
    }
}
