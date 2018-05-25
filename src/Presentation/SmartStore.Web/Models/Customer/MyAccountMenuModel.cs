using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Models.Customer
{
    public partial class MyAccountMenuModel : ModelBase
    {
		public TreeNode<MenuItem> Root { get; set; }
        public string SelectedItemToken { get; set; }
    }
}