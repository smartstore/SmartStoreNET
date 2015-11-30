using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
	
	public class NavigationModel
	{
		public TreeNode<MenuItem> Root { get; set; }
		public IList<MenuItem> Path { get; set; }

		public MenuItem SelectedMenuItem
		{
			get
			{
				if (Path == null || Path.Count == 0)
					return null;

				return Path.Last();
			}
		}
	}

}
