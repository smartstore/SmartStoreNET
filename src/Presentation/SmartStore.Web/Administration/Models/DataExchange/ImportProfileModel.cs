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
		public ImportProfileModel()
		{
			ExtraData = new ExtraDataModel();
		}

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
		public int? Skip { get; set; }

		[SmartResourceDisplayName("Admin.Common.RecordsTake")]
		public int? Take { get; set; }

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

		[SmartResourceDisplayName("Admin.DataExchange.Import.LastImportResult")]
		public SerializableImportResult ImportResult { get; set; }

		public bool LogFileExists { get; set; }
		public string TempFileName { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Import.FolderName")]
		public string FolderName { get; set; }

		public CsvConfigurationModel CsvConfiguration { get; set; }

		public ExtraDataModel ExtraData { get; set; }

		public List<ColumnMappingItemModel> ColumnMappings { get; set; }
		public List<ColumnMappingItemModel> AvailableSourceColumns { get; set; }
		public List<ColumnMappingItemModel> AvailableEntityProperties { get; set; }

		public int MaxMappingLabelTextLength { get { return 42; } }

		public class ExtraDataModel
		{
			[SmartResourceDisplayName("Admin.DataExchange.Import.NumberOfPictures")]
			public int? NumberOfPictures { get; set; }
		}
	}


	public class ColumnMappingItemModel
	{
		public int Index { get; set; }

		public string Column { get; set; }
		public string ColumnWithoutIndex { get; set; }
		public string ColumnIndex { get; set; }

		public string Property { get; set; }
		public string PropertyDescription { get; set; }

		public string Default { get; set; }
		public bool IsDefaultDisabled { get; set; }
	}
}