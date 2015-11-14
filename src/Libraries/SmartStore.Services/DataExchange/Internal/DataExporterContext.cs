using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Internal
{
	internal class DataExporterContext
	{
		public DataExporterContext(
			DataExportRequest request,
			CancellationToken cancellationToken,
			bool isPreview = false)
		{
			Request = request;
			CancellationToken = cancellationToken;
			Filter = XmlHelper.Deserialize<ExportFilter>(request.Profile.Filtering);
			Projection = XmlHelper.Deserialize<ExportProjection>(request.Profile.Projection);
			IsPreview = isPreview;

			FolderContent = FileSystemHelper.TempDir(@"Profile\Export\{0}\Content".FormatInvariant(request.Profile.FolderName));
			FolderRoot = System.IO.Directory.GetParent(FolderContent).FullName;

			Categories = new Dictionary<int, Category>();
			CategoryPathes = new Dictionary<int, string>();
			Countries = new Dictionary<int, Country>();
			ProductTemplates = new Dictionary<int, ProductTemplate>();
			NewsletterSubscriptions = new HashSet<string>();

			RecordsPerStore = new Dictionary<int, int>();
			EntityIdsLoaded = new List<int>();
			EntityIdsPerSegment = new List<int>();

			Result = new DataExportResult
			{
				FileFolder = (IsFileBasedExport ? FolderContent : null)
			};

			ExecuteContext = new ExportExecuteContext(Result, CancellationToken, FolderContent);
			ExecuteContext.Projection = XmlHelper.Deserialize<ExportProjection>(request.Profile.Projection);
		}

		/// <summary>
		/// All entity identifiers per export
		/// </summary>
		public List<int> EntityIdsLoaded { get; set; }
		public void SetLoadedEntityIds(IEnumerable<int> ids)
		{
			EntityIdsLoaded = EntityIdsLoaded
				.Union(ids)
				.Distinct()
				.ToList();
		}

		/// <summary>
		/// All entity identifiers per segment (to avoid exporting products multiple times)
		/// </summary>
		public List<int> EntityIdsPerSegment { get; set; }

		public int RecordCount { get; set; }
		public Dictionary<int, int> RecordsPerStore { get; set; }
		public string ProgressInfo { get; set; }

		public DataExportRequest Request { get; private set; }
		public CancellationToken CancellationToken { get; private set; }
		public bool IsPreview { get; private set; }

		public bool Supports(ExportFeatures feature)
		{
			return (!IsPreview && Request.Provider.Metadata.ExportFeatures.HasFlag(feature));
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
			get { return Request.Profile.FolderName + ".zip"; }
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
			get { return Request.Provider == null || Request.Provider.Value == null || Request.Provider.Value.FileExtension.HasValue(); }
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
		public ProductExportContext ProductExportContext { get; set; }
		public OrderExportContext OrderExportContext { get; set; }
		public ManufacturerExportContext ManufacturerExportContext { get; set; }
		public CategoryExportContext CategoryExportContext { get; set; }
		public CustomerExportContext CustomerExportContext { get; set; }

		public ExportExecuteContext ExecuteContext { get; set; }
		public DataExportResult Result { get; set; }
	}
}
