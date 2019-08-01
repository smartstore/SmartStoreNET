using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Collections;
using SmartStore.Core.Domain.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Security
{
    public class PermissionListModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.Customers.Customers.CustomerRole")]
        public int RoleId { get; set; }
        public IList<SelectListItem> Roles { get; set; }

        public TreeNode<IPermissionNode> PermissionTree { get; set; }
    }
}