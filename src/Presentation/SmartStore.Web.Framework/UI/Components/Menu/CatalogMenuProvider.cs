using System;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
	public abstract class CatalogMenuProvider : IMenuProvider
	{
		public abstract void BuildMenu(TreeNode<MenuItem> rootNode);

		public string MenuName
		{
			get { return "catalog"; }
		}

		public virtual int Ordinal
		{
			get { return 0; }
		}

	}
}
