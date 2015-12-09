using System.Collections.Generic;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Admin.Validators.DataExchange;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.DataExchange
{
	[Validator(typeof(ImportProfileValidator))]
	public partial class ImportProfileModel : EntityModelBase
	{
		[SmartResourceDisplayName("Admin.DataExchange.Import.Name")]
		public string Name { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Import.FolderName")]
		public string FolderName { get; set; }

		[SmartResourceDisplayName("Common.Files")]
		public List<string> FileNames { get; set; }

		[SmartResourceDisplayName("Admin.Common.Entity")]
		public string EntityType { get; set; }

		[SmartResourceDisplayName("Common.Enabled")]
		public bool Enabled { get; set; }

		[SmartResourceDisplayName("Admin.Common.RecordsSkip")]
		public int Skip { get; set; }

		[SmartResourceDisplayName("Admin.Common.RecordsTake")]
		public int Take { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Import.Cleanup")]
		public bool Cleanup { get; set; }

		[SmartResourceDisplayName("Common.Execution")]
		public int ScheduleTaskId { get; set; }
		public string ScheduleTaskName { get; set; }
		public bool IsTaskRunning { get; set; }
		public bool IsTaskEnabled { get; set; }

		public ScheduleTaskModel TaskModel { get; set; }
		public bool LogFileExists { get; set; }
	}
}