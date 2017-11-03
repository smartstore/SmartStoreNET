using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Export.Internal
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

			if (request.Profile.Projection.IsEmpty())
			{
				Projection.DescriptionMergingId = (int)ExportDescriptionMerging.Description;
			}

			FolderContent = request.Profile.GetExportFolder(true, true);

			DeliveryTimes = new Dictionary<int, DeliveryTime>();
			QuantityUnits = new Dictionary<int, QuantityUnit>();
			Stores = new Dictionary<int, Store>();
			Languages = new Dictionary<int, Language>();
			Countries = new Dictionary<int, Country>();
			ProductTemplates = new Dictionary<int, string>();
			CategoryTemplates = new Dictionary<int, string>();
			NewsletterSubscriptions = new HashSet<string>();

			RecordsPerStore = new Dictionary<int, int>();
			EntityIdsLoaded = new List<int>();
			EntityIdsPerSegment = new List<int>();

			Result = new DataExportResult
			{
				FileFolder = (IsFileBasedExport ? FolderContent : null)
			};

			ExecuteContext = new ExportExecuteContext(Result, CancellationToken, FolderContent);
			ExecuteContext.Filter = Filter;
			ExecuteContext.Projection = Projection;
			ExecuteContext.ProfileId = request.Profile.Id;

			if (!IsPreview)
			{
				ExecuteContext.ProgressValueSetter = Request.ProgressValueSetter;
			}
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

		public string FolderContent { get; private set; }

		public bool IsFileBasedExport
		{
			get { return Request.Provider == null || Request.Provider.Value == null || Request.Provider.Value.FileExtension.HasValue(); }
		}

		// data loaded once per export
		public Dictionary<int, DeliveryTime> DeliveryTimes { get; set; }
		public Dictionary<int, QuantityUnit> QuantityUnits { get; set; }
		public Dictionary<int, Store> Stores { get; set; }
		public Dictionary<int, Language> Languages { get; set; }
		public Dictionary<int, Country> Countries { get; set; }
		public Dictionary<int, string> ProductTemplates { get; set; }
		public Dictionary<int, string> CategoryTemplates { get; set; }
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
