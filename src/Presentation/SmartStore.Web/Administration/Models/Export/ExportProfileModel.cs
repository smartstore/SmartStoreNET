using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Export
{
	public partial class ExportProfileModel : EntityModelBase
	{
		[SmartResourceDisplayName("Admin.Configuration.Export.Name")]
		public string Name { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.ProviderSystemName")]
		public string ProviderSystemName { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.EntityType")]
		public string EntityType { get; set; }

		[SmartResourceDisplayName("Common.Enabled")]
		public bool Enabled { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.FileType")]
		public string FileType { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.SchedulingHours")]
		public int SchedulingHours { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.LastExecution")]
		[AllowHtml]
		public string LastExecution { get; set; }
	}
}