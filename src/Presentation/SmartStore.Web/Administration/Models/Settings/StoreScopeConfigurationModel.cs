using System.Collections.Generic;
using SmartStore.Admin.Models.Stores;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Settings
{
	public partial class StoreScopeConfigurationModel : ModelBase
	{
		public StoreScopeConfigurationModel()
		{
			Stores = new List<StoreModel>();
		}

		public int StoreId { get; set; }
		public IList<StoreModel> Stores { get; set; }
	}
}