using System.Web;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Localization;
using SmartStore.Services.Catalog;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using SmartStore.Services.Tasks;
using SmartStore.Services.Tax;

namespace SmartStore.Services.DataExchange.Internal
{
	// TODO: internal really possible here because of IOC?
	internal partial class DataExportTask : ITask
	{
		private readonly ICommonServices _services;
		private readonly IDataExporter _exporter;
		private readonly IExportProfileService _exportProfileService;
		private readonly IUrlRecordService _urlRecordService;
		private readonly ILocalizedEntityService _localizedEntityService;
		private readonly IPictureService _pictureService;
		private readonly IPriceCalculationService _priceCalculationService;
		private readonly ICurrencyService _currencyService;
		private readonly ITaxService _taxService;
		private readonly IPriceFormatter _priceFormatter;
		private readonly ICategoryService _categoryService;
		private readonly IProductAttributeParser _productAttributeParser;

		private MediaSettings _mediaSettings;

		public DataExportTask(
			ICommonServices services,
			IDataExporter exporter,
			IExportProfileService exportProfileService,
			IUrlRecordService urlRecordService,
			ILocalizedEntityService localizedEntityService,
			IPictureService pictureService,
			IPriceCalculationService priceCalculationService,
			ICurrencyService currencyService,
			ITaxService taxService,
			IPriceFormatter priceFormatter,
			ICategoryService categoryService,
			IProductAttributeParser productAttributeParser,
            MediaSettings mediaSettings)
		{
			_services = services;
			_exporter = exporter;
			_exportProfileService = exportProfileService;
			_urlRecordService = urlRecordService;
			_localizedEntityService = localizedEntityService;
			_pictureService = pictureService;
			_priceCalculationService = priceCalculationService;
			_currencyService = currencyService;
			_taxService = taxService;
			_priceFormatter = priceFormatter;
			_categoryService = categoryService;
			_productAttributeParser = productAttributeParser;

			_mediaSettings = mediaSettings;
        }

		public Localizer T { get; set; }

		public void Execute(TaskExecutionContext ctx)
		{
			// TODO: proper error handling

			var profileId = ctx.ScheduleTask.Alias.ToInt();
			var profile = _exportProfileService.GetExportProfileById(profileId);

			// TODO: find a better way to transmit selected entity ids (e.g. new TaskExecutionContext.Parameters property)
			var selectedIdsCacheKey = profile.GetSelectedEntityIdsCacheKey();
			var selectedEntityIds = HttpRuntime.Cache[selectedIdsCacheKey] as string;
			HttpRuntime.Cache.Remove(selectedIdsCacheKey);

			// load provider
			var provider = _exportProfileService.LoadProvider(profile.ProviderSystemName);
			if (provider == null)
				throw new SmartException(T("Admin.Common.ProviderNotLoaded", profile.ProviderSystemName.NaIfEmpty()));

			// build export request
			var request = new DataExportRequest(profile);
			request.ProgressSetter = delegate(int val, int max, string msg)
			{
				ctx.SetProgress(val, max, msg);
			};
			if (selectedEntityIds.HasValue())
			{
				request.EntitiesToExport = selectedEntityIds.ToIntArray();
            }

			// process!
			_exporter.Export(request, ctx.CancellationToken);

			ctx.CancellationToken.ThrowIfCancellationRequested();
		}
	}
}
