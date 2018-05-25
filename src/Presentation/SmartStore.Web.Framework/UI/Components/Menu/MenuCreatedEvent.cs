using System;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
	public class MenuCreatedEvent
	{
		public MenuCreatedEvent(string menuName, TreeNode<MenuItem> root, string selectedItemToken)
		{
			Guard.NotEmpty(menuName, nameof(menuName));
			Guard.NotNull(root, nameof(root));

			MenuName = menuName;
			Root = root;
			SelectedItemToken = selectedItemToken;
		}

		public string MenuName { get; private set; }
		public TreeNode<MenuItem> Root { get; private set; }
		public string SelectedItemToken { get; set; }
	}
}
