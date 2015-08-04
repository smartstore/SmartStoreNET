using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.DataExchange
{
	public partial class ExportProfileModel : EntityModelBase
	{
		[SmartResourceDisplayName("Admin.Configuration.Export.Name")]
		public string Name { get; set; }

		[SmartResourceDisplayName("Common.Enabled")]
		public bool Enabled { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.SchedulingHours")]
		public int SchedulingHours { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.LastExecution")]
		[AllowHtml]
		public string LastExecution { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Offset")]
		public int Offset { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Limit")]
		public int Limit { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.BatchSize")]
		public int BatchSize { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.PerStore")]
		public bool PerStore { get; set; }

		public Provider Providing { get; set; }

		public ExportProductFilterModel ProductFiltering { get; set; }
		public ExportOrderFilterModel OrderFiltering { get; set; }

		public int StoreCount { get; set; }
		public string AllString { get; set; }


		public class Provider
		{
			[SmartResourceDisplayName("Common.Image")]
			public string ThumbnailUrl { get; set; }

			[SmartResourceDisplayName("Common.Website")]
			public string Url { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.Configure")]
			public string ConfigurationUrl { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Export.ProviderSystemName")]
			public string SystemName { get; set; }
			public List<SelectListItem> AvailableExportProviders { get; set; }

			[SmartResourceDisplayName("Common.Provider")]
			public string FriendlyName { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.Author")]
			public string Author { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.Version")]
			public string Version { get; set; }

			[SmartResourceDisplayName("Common.Description")]
			[AllowHtml]
			public string Description { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Export.EntityType")]
			public ExportEntityType EntityType { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Export.EntityType")]
			public string EntityTypeName { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Export.FileType")]
			public string FileType { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Export.SupportedFileTypes")]
			public string SupportedFileTypes { get; set; }
		}
	}
}