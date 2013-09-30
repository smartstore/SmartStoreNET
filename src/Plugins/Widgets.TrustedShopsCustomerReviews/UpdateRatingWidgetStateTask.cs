using SmartStore.Core;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Logging;
using SmartStore.Services.Tasks;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews
{
    public class UpdateRatingWidgetStateTask : ITask
    {
        
        private readonly TrustedShopsCustomerReviewsSettings _trustedShopsCustomerReviewsSettings;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;

        public UpdateRatingWidgetStateTask(TrustedShopsCustomerReviewsSettings trustedShopsCustomerReviewsSettings,
            ISettingService settingService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ILogger logger)
        {
            _trustedShopsCustomerReviewsSettings = trustedShopsCustomerReviewsSettings;
            _settingService = settingService;
            _localizationService = localizationService;
            _workContext = workContext;
            _logger = logger;
        }

		/// <summary>
		/// Execute task
		/// </summary>
		public void Execute() {

            var tsProtectionServiceSandbox = new TrustedShopsCustomerReviews.com.trustedshops.qa.TSRatingService();
            var tsProtectionServiceLive = new TrustedShopsCustomerReviews.com.trustedshops.www.TSRatingService();
            string response = "";

            if (_trustedShopsCustomerReviewsSettings.IsTestMode)
            {
                response = tsProtectionServiceSandbox.updateRatingWidgetState(
                    _trustedShopsCustomerReviewsSettings.TrustedShopsId,
                    _trustedShopsCustomerReviewsSettings.ActivationState,
                    "smart-store-ag",
                    "pWnysOMd",
                    "SmartStore.NET");
            }
            else
            {
                response = tsProtectionServiceLive.updateRatingWidgetState(
                    _trustedShopsCustomerReviewsSettings.TrustedShopsId,
                    _trustedShopsCustomerReviewsSettings.ActivationState,
                    "smart-store-ag",
                    "pWnysOMd",
                    "SmartStore.NET");
            }

            switch (response)
            {
                case "OK":
                    _logger.InsertLog(LogLevel.Information, _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerReviews.CheckIdSuccess"));
                    break;
                case "INVALID_TSID":
                    _logger.InsertLog(LogLevel.Information, _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerReviews.InvalidId"));
                    break;
                case "NOT_REGISTERED_FOR_TRUSTEDRATING":
                    _logger.InsertLog(LogLevel.Information, _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerReviews.NotRegistered"));
                    break;
                case "WRONG_WSUSERNAME_WSPASSWORD":
                    _logger.InsertLog(LogLevel.Information, _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerReviews.WrongCredentials"));
                    break;
                default:
                    _logger.InsertLog(LogLevel.Information, _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerReviews.OtherError"));
                    break;
            }

		}

    }
}