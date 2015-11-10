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

namespace SmartStore.Services.DataExchange.Internal
{
	internal class DataExportTaskContext
	{
		private ProductExportContext _productExportContext;
		private OrderExportContext _orderExportContext;
		private ManufacturerExportContext _manufacturerExportContext;
		private CategoryExportContext _categoryExportContext;
		private CustomerExportContext _customerExportContext;

		public DataExportTaskContext(
			TaskExecutionContext taskContext,
			ExportProfile profile,
			Provider<IExportProvider> provider,
			string selectedIds = null,
			Action<dynamic> previewData = null)
		{
			TaskContext = taskContext;
			Profile = profile;
			Provider = provider;
			Filter = XmlHelper.Deserialize<ExportFilter>(profile.Filtering);
			Projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);
			EntityIdsSelected = selectedIds.SplitSafe(",").Select(x => x.ToInt()).ToList();
			PreviewData = previewData;

			FolderContent = FileSystemHelper.TempDir(@"Profile\Export\{0}\Content".FormatInvariant(profile.FolderName));
			FolderRoot = System.IO.Directory.GetParent(FolderContent).FullName;

			Categories = new Dictionary<int, Category>();
			CategoryPathes = new Dictionary<int, string>();
			Countries = new Dictionary<int, Country>();
			ProductTemplates = new Dictionary<int, ProductTemplate>();
			NewsletterSubscriptions = new HashSet<string>();

			RecordsPerStore = new Dictionary<int, int>();
			EntityIdsLoaded = new List<int>();

			Result = new DataExportResult
			{
				FileFolder = (IsFileBasedExport ? FolderContent : null)
			};

			ExecuteContext = new ExportExecuteContext(Result, TaskContext.CancellationToken, FolderContent);
			ExecuteContext.Projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);
		}

		public List<int> EntityIdsSelected { get; private set; }
		public List<int> EntityIdsLoaded { get; set; }

		public int RecordCount { get; set; }
		public Dictionary<int, int> RecordsPerStore { get; set; }
		public string ProgressInfo { get; set; }
		public IQueryable<Product> QueryProducts { get; set; }

		public Action<dynamic> PreviewData { get; private set; }
		public bool IsPreview
		{
			get { return PreviewData != null; }
		}

		public TaskExecutionContext TaskContext { get; private set; }
		public ExportProfile Profile { get; private set; }
		public Provider<IExportProvider> Provider { get; private set; }

		public bool Supports(ExportFeature feature)
		{
			return (!IsPreview && Provider.Metadata.ExportFeature.HasFlag(feature));
		}

		public ExportFilter Filter { get; private set; }
		public ExportProjection Projection { get; private set; }
		public Currency ContextCurrency { get; set; }
		public Customer ContextCustomer { get; set; }
		public Language ContextLanguage { get; set; }

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
			get { return Provider == null || Provider.Value == null || Provider.Value.FileExtension.HasValue(); }
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
		public Dictionary<int, Language> Languages { get; set; }
		public Dictionary<int, Country> Countries { get; set; }
		public Dictionary<int, ProductTemplate> ProductTemplates { get; set; }
		public HashSet<string> NewsletterSubscriptions { get; set; }

		// data loaded once per page
		public ProductExportContext ProductExportContext
		{
			get
			{
				return _productExportContext;
			}
			set
			{
				if (_productExportContext != null)
					_productExportContext.Clear();

				_productExportContext = value;
			}
		}

		public OrderExportContext OrderExportContext
		{
			get
			{
				return _orderExportContext;
			}
			set
			{
				if (_orderExportContext != null)
					_orderExportContext.Clear();

				_orderExportContext = value;
			}
		}

		public ManufacturerExportContext ManufacturerExportContext
		{
			get
			{
				return _manufacturerExportContext;
			}
			set
			{
				if (_manufacturerExportContext != null)
					_manufacturerExportContext.Clear();

				_manufacturerExportContext = value;
			}
		}

		public CategoryExportContext CategoryExportContext
		{
			get
			{
				return _categoryExportContext;
			}
			set
			{
				if (_categoryExportContext != null)
					_categoryExportContext.Clear();

				_categoryExportContext = value;
			}
		}
		public CustomerExportContext CustomerExportContext
		{
			get
			{
				return _customerExportContext;
			}
			set
			{
				if (_customerExportContext != null)
					_customerExportContext.Clear();

				_customerExportContext = value;
			}
		}

		public ExportExecuteContext ExecuteContext { get; set; }
		public DataExportResult Result { get; set; }
	}
}
