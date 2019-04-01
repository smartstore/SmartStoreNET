using SmartStore.Collections;
using SmartStore.Core.Domain.Cms;

namespace SmartStore.Web.Framework.UI
{
    public interface IMenuItemProvider
	{
        /// <summary>
        /// Converts a <see cref="MenuItemRecord"/> object and appends it to the parent tree node.
        /// </summary>
        /// <param name="request">Contains information about the request to the provider.</param>
		void Append(MenuItemProviderRequest request);
	}


    public class MenuItemProviderRequest
    {
        public string Origin { get; set; }

        public TreeNode<MenuItem> Parent { get; set; }

        public MenuItemRecord Entity { get; set; }
    }
}
