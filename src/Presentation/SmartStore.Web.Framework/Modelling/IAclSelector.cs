using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Modelling
{
	public interface IAclSelector
	{
		[SmartResourceDisplayName("Admin.Common.Acl.SubjectTo")]
		bool SubjectToAcl { get; }

		[SmartResourceDisplayName("Admin.Common.Acl.AvailableFor")]
		IEnumerable<SelectListItem> AvailableCustomerRoles { get; }

		int[] SelectedCustomerRoleIds { get; }
	}
}
