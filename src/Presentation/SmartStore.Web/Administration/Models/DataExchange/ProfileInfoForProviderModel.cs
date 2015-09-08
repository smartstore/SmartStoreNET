using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.DataExchange
{
	public class ProfileInfoForProviderModel : ModelBase
	{
		[SmartResourceDisplayName("Admin.DataExchange.Export.ProfileForProvider")]
		public List<ProfileModel> Profiles { get; set; }

		[SmartResourceDisplayName("Common.Execution")]
		public int? ScheduleTaskId { get; set; }
		
		public string ReturnUrl { get; set; }

		public class ProfileModel : EntityModelBase
		{
			public bool Enabled { get; set; }

			public string Name { get; set; }
		}
	}
}