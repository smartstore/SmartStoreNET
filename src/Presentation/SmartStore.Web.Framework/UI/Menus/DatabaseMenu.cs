using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Logging;
using SmartStore.Core.Domain.Cms;
using SmartStore.Collections;
using SmartStore.Services;
using SmartStore.Services.Cms;

namespace SmartStore.Web.Framework.UI
{
	/// <summary>
	/// A generic implementation of <see cref="IMenu" /> which represents a
	/// <see cref="MenuRecord"/> entity retrieved by <see cref="IMenuStorage"/>.
	/// </summary>
	internal class DatabaseMenu : IMenu
	{
		public string Name => throw new NotImplementedException();

		public TreeNode<MenuItem> Root => throw new NotImplementedException();

		public bool ApplyPermissions => throw new NotImplementedException();

		public void ClearCache()
		{
			throw new NotImplementedException();
		}

		public IDictionary<string, TreeNode<MenuItem>> GetAllCachedMenus()
		{
			throw new NotImplementedException();
		}

		public void ResolveElementCounts(TreeNode<MenuItem> curNode, bool deep = false)
		{
			throw new NotImplementedException();
		}
	}
}