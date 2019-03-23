using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core.Domain.Cms;

namespace SmartStore.Web.Framework.UI
{
	public interface IMenuItemProvider
	{
		void Append(TreeNode<MenuItem> parent, MenuItemRecord entity);

		// TODO: implement dependency registration mechanism similar to PageBuilder blocks.
	}
}
