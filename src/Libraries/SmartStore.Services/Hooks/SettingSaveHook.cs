using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Hooks
{
    public class SettingSaveHook : DbSaveHook<Setting>
    {
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        public SettingSaveHook(ICustomerActivityService customerActivityService, ILocalizationService localizationService)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
        }

        protected override void OnUpdated(Setting entity, IHookedEntity entry)
        {
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"), entity.Name, entity.Value);
        }
    }
}
