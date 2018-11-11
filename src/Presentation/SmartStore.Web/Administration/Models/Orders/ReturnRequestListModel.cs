using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Orders;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
	public class ReturnRequestListModel : ModelBase
	{
		public ReturnRequestListModel()
		{
			AvailableStores = new List<SelectListItem>();
			AvailableReturnRequestStatus = new List<SelectListItem>();
		}

		public int GridPageSize { get; set; }

		[SmartResourceDisplayName("Admin.ReturnRequests.Fields.ID")]
		public int? SearchId { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
		public int SearchStoreId { get; set; }
		public IList<SelectListItem> AvailableStores { get; set; }

		[SmartResourceDisplayName("Admin.ReturnRequests.Fields.Status")]
		public int? SearchReturnRequestStatusId { get; set; }
		public IList<SelectListItem> AvailableReturnRequestStatus { get; set; }

		public ReturnRequestStatus? SearchReturnRequestStatus
		{
			get
			{
				if (SearchReturnRequestStatusId.HasValue)
					return (ReturnRequestStatus)SearchReturnRequestStatusId.Value;
				return null;
			}
		}
	}
}