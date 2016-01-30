using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Admin.Validators.DataExchange;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.DataExchange
{
	[Validator(typeof(ImportProfileValidator))]
	public partial class ImportProfileModel : EntityModelBase
	{
		[SmartResourceDisplayName("Admin.DataExchange.Import.Name")]
		public string Name { get; set; }

		[SmartResourceDisplayName("Admin.Common.ImportFiles")]
		public List<string> ExistingFileNames { get; set; }

		[SmartResourceDisplayName("Admin.Common.Entity")]
		public ImportEntityType EntityType { get; set; }

		[SmartResourceDisplayName("Admin.Common.Entity")]
		public string EntityTypeName { get; set; }

		[SmartResourceDisplayName("Common.Enabled")]
		public bool Enabled { get; set; }

		[SmartResourceDisplayName("Admin.Common.RecordsSkip")]
		public int Skip { get; set; }

		[SmartResourceDisplayName("Admin.Common.RecordsTake")]
		public int Take { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Import.UpdateOnly")]
		public bool UpdateOnly { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Import.KeyFieldNames")]
		public string[] KeyFieldNames { get; set; }
		public List<SelectListItem> AvailableKeyFieldNames { get; set; }

		[SmartResourceDisplayName("Common.Execution")]
		public int ScheduleTaskId { get; set; }
		public string ScheduleTaskName { get; set; }
		public bool IsTaskRunning { get; set; }
		public bool IsTaskEnabled { get; set; }

		public ScheduleTaskModel TaskModel { get; set; }
		public SerializableImportResult ImportResult { get; set; }

		public bool LogFileExists { get; set; }
		public string TempFileName { get; set; }
		public string UnspecifiedString { get; set; }
		public string AddNewString { get; set; }
		public string DeleteString { get; set; }
		public string IgnoreString { get; set; }

		public CsvConfigurationModel CsvConfiguration { get; set; }

		public List<ColumnMappingItemModel> ColumnMappings { get; set; }
		public List<ColumnMappingItemModel> AvailableSourceColumns { get; set; }
		public List<SelectListItem> AvailableEntityProperties { get; set; }
	}


	public class ColumnMappingItemModel
	{
		public int Index { get; set; }

		public string Column { get; set; }
		public string ColumnWithoutIndex { get; set; }
		public string ColumnIndex { get; set; }
		public string ColumnLocalized { get; set; }

		public string Property { get; set; }
		public string Default { get; set; }
	}
}