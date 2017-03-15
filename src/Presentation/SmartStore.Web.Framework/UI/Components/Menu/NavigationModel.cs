using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
	public class NavigationModel
	{
		private TreeNode<MenuItem> _selectedNode;
		private bool _seekedSelectedNode;

		public TreeNode<MenuItem> Root { get; set; }
		public IList<TreeNode<MenuItem>> Path { get; set; }

		public TreeNode<MenuItem> SelectedNode
		{
			get
			{
				if (!_seekedSelectedNode)
				{
					_selectedNode = Path?.LastOrDefault() ?? Root;
					_seekedSelectedNode = true;
				}

				return _selectedNode;
			}
		}
	}
}
