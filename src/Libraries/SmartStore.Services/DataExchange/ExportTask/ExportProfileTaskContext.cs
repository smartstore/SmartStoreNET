using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.ExportTask
{
	internal class ExportProfileTaskContext
	{
		private ExportProductDataContext _productDataContext;
		private ExportOrderDataContext _orderDataContext;

		public ExportProfileTaskContext(
			TaskExecutionContext taskContext,
			ExportProfile profile,
			Provider<IExportProvider> provider,
			string selectedIds = null,
			int pageIndex = 0,
			int pageSize = 100,
			int? totalRecords = null,
			Action<dynamic> previewData = null)
		{
			TaskContext = taskContext;
			Profile = profile;
			Provider = provider;
			Filter = XmlHelper.Deserialize<ExportFilter>(profile.Filtering);
			Projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);
			EntityIdsSelected = selectedIds.SplitSafe(",").Select(x => x.ToInt()).ToList();
			PageIndex = pageIndex;
			PageSize = pageSize;
			TotalRecords = totalRecords;
			PreviewData = previewData;

			FolderContent = FileSystemHelper.TempDir(@"Profile\Export\{0}\Content".FormatInvariant(profile.FolderName));
			FolderRoot = System.IO.Directory.GetParent(FolderContent).FullName;

			Categories = new Dictionary<int, Category>();
			CategoryPathes = new Dictionary<int, string>();
			Countries = new Dictionary<int, Country>();
			ProductTemplates = new Dictionary<int, ProductTemplate>();

			RecordsPerStore = new Dictionary<int, int>();
			EntityIdsLoaded = new List<int>();

			Export = new ExportExecuteContext(TaskContext.CancellationToken, FolderContent);
			Export.Projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);

			Result = new ExportExecuteResult
			{
				FileFolder = (IsFileBasedExport ? FolderContent : null)
			};
		}

		public List<int> EntityIdsSelected { get; private set; }
		public List<int> EntityIdsLoaded { get; set; }

		public int PageIndex { get; private set; }
		public int PageSize { get; private set; }
		public int? TotalRecords { get; set; }
		public int RecordCount { get; set; }
		public Dictionary<int, int> RecordsPerStore { get; set; }
		public string ProgressInfo { get; set; }

		public Action<dynamic> PreviewData { get; private set; }
		public bool IsPreview
		{
			get { return PreviewData != null; }
		}

		public TaskExecutionContext TaskContext { get; private set; }
		public ExportProfile Profile { get; private set; }
		public Provider<IExportProvider> Provider { get; private set; }

		public ExportFilter Filter { get; private set; }
		public ExportProjection Projection { get; private set; }
		public Currency ProjectionCurrency { get; set; }
		public Customer ProjectionCustomer { get; set; }
		public Language ProjectionLanguage { get; set; }

		public TraceLogger Log { get; set; }
		public Store Store { get; set; }

		public string FolderRoot { get; private set; }
		public string FolderContent { get; private set; }
		public string ZipName
		{
			get { return Profile.FolderName + ".zip"; }
		}
		public string ZipPath
		{
			get { return Path.Combine(FolderRoot, ZipName); }
		}
		public string LogPath
		{
			get { return Path.Combine(FolderRoot, "log.txt"); }
		}

		public bool IsFileBasedExport
		{
			get { return Provider.Value.FileExtension.HasValue(); }
		}
		public string[] GetDeploymentFiles(ExportDeployment deployment)
		{
			if (!IsFileBasedExport)
				return new string[0];

			if (deployment.CreateZip)
				return new string[] { ZipPath };

			return System.IO.Directory.GetFiles(FolderContent, "*.*", SearchOption.AllDirectories);
		}

		// data loaded once per export
		public Dictionary<int, Category> Categories { get; set; }
		public Dictionary<int, string> CategoryPathes { get; set; }
		public Dictionary<int, DeliveryTime> DeliveryTimes { get; set; }
		public Dictionary<int, QuantityUnit> QuantityUnits { get; set; }
		public Dictionary<int, Store> Stores { get; set; }
		public Dictionary<int, Country> Countries { get; set; }
		public Dictionary<int, ProductTemplate> ProductTemplates { get; set; }

		// data loaded once per page
		public ExportProductDataContext ProductDataContext
		{
			get
			{
				return _productDataContext;
			}
			set
			{
				if (_productDataContext != null)
					_productDataContext.Clear();

				_productDataContext = value;
			}
		}
		public ExportOrderDataContext OrderDataContext
		{
			get
			{
				return _orderDataContext;
			}
			set
			{
				if (_orderDataContext != null)
					_orderDataContext.Clear();

				_orderDataContext = value;
			}
		}

		public ExportExecuteContext Export { get; set; }
		public ExportExecuteResult Result { get; set; }
	}
}
