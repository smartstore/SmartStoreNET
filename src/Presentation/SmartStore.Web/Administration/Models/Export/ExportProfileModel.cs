using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Export;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Export
{
	public partial class ExportProfileModel : EntityModelBase
	{
		[SmartResourceDisplayName("Common.Image")]
		public string ThumbnailUrl { get; set; }
		public string ThumbnailClasses
		{
			get
			{
				return "grid-icon";
			}
		}

		[SmartResourceDisplayName("Admin.Configuration.Export.ProviderSystemName")]
		public string ProviderSystemName { get; set; }
		public List<SelectListItem> AvailableExportProviders { get; set; }

		[SmartResourceDisplayName("Common.Provider")]
		public string ProviderFriendlyName { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.EntityType")]
		public string EntityType { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.SupportedFileTypes")]
		public string SupportedFileTypes { get; set; }


		[SmartResourceDisplayName("Admin.Configuration.Export.Name")]
		public string Name { get; set; }

		[SmartResourceDisplayName("Common.Enabled")]
		public bool Enabled { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.FileType")]
		public ExportFileType FileType { get; set; }
		public List<SelectListItem> AvailableFileTypes { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.FileType")]
		public string FileTypeName { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.SchedulingHours")]
		public int SchedulingHours { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.LastExecution")]
		[AllowHtml]
		public string LastExecution { get; set; }


		[SmartResourceDisplayName("Admin.Configuration.Export.Segmentation.Offset")]
		public int Offset { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Segmentation.Limit")]
		public int Limit { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Segmentation.BatchSize")]
		public int BatchSize { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Segmentation.PerStore")]
		public bool PerStore { get; set; }
	}
}