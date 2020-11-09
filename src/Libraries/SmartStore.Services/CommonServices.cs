using System;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services.Configuration;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Stores;

namespace SmartStore.Services
{
    public class CommonServices : ICommonServices
    {
        private readonly IComponentContext _container;
        private readonly Lazy<IApplicationEnvironment> _env;
        private readonly Lazy<ICacheManager> _cacheManager;
        private readonly Lazy<IRequestCache> _requestCache;
        private readonly Lazy<IDbContext> _dbContext;
        private readonly Lazy<IStoreContext> _storeContext;
        private readonly Lazy<IWebHelper> _webHelper;
        private readonly Lazy<IWorkContext> _workContext;
        private readonly Lazy<IEventPublisher> _eventPublisher;
        private readonly Lazy<ILocalizationService> _localization;
        private readonly Lazy<ICustomerActivityService> _customerActivity;
        private readonly Lazy<IMediaService> _mediaService;
        private readonly Lazy<INotifier> _notifier;
        private readonly Lazy<IPermissionService> _permissions;
        private readonly Lazy<ISettingService> _settings;
        private readonly Lazy<IStoreService> _storeService;
        private readonly Lazy<IDateTimeHelper> _dateTimeHelper;
        private readonly Lazy<IDisplayControl> _displayControl;
        private readonly Lazy<IChronometer> _chronometer;
        private readonly Lazy<IMessageFactory> _messageFactory;

        public CommonServices(
            IComponentContext container,
            Lazy<IApplicationEnvironment> env,
            Lazy<ICacheManager> cacheManager,
            Lazy<IRequestCache> requestCache,
            Lazy<IDbContext> dbContext,
            Lazy<IStoreContext> storeContext,
            Lazy<IWebHelper> webHelper,
            Lazy<IWorkContext> workContext,
            Lazy<IEventPublisher> eventPublisher,
            Lazy<ILocalizationService> localization,
            Lazy<ICustomerActivityService> customerActivity,
            Lazy<IMediaService> mediaService,
            Lazy<INotifier> notifier,
            Lazy<IPermissionService> permissions,
            Lazy<ISettingService> settings,
            Lazy<IStoreService> storeService,
            Lazy<IDateTimeHelper> dateTimeHelper,
            Lazy<IDisplayControl> displayControl,
            Lazy<IChronometer> chronometer,
            Lazy<IMessageFactory> messageFactory)
        {
            _container = container;
            _env = env;
            _cacheManager = cacheManager;
            _requestCache = requestCache;
            _dbContext = dbContext;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _eventPublisher = eventPublisher;
            _localization = localization;
            _customerActivity = customerActivity;
            _mediaService = mediaService;
            _notifier = notifier;
            _permissions = permissions;
            _settings = settings;
            _storeService = storeService;
            _dateTimeHelper = dateTimeHelper;
            _displayControl = displayControl;
            _chronometer = chronometer;
            _messageFactory = messageFactory;
        }

        public IComponentContext Container => _container;
        public IApplicationEnvironment ApplicationEnvironment => _env.Value;
        public ICacheManager Cache => _cacheManager.Value;
        public IRequestCache RequestCache => _requestCache.Value;
        public IDbContext DbContext => _dbContext.Value;
        public IStoreContext StoreContext => _storeContext.Value;
        public IWebHelper WebHelper => _webHelper.Value;
        public IWorkContext WorkContext => _workContext.Value;
        public IEventPublisher EventPublisher => _eventPublisher.Value;
        public ILocalizationService Localization => _localization.Value;
        public ICustomerActivityService CustomerActivity => _customerActivity.Value;
        public IMediaService MediaService => _mediaService.Value;
        public INotifier Notifier => _notifier.Value;
        public IPermissionService Permissions => _permissions.Value;
        public ISettingService Settings => _settings.Value;
        public IStoreService StoreService => _storeService.Value;
        public IDateTimeHelper DateTimeHelper => _dateTimeHelper.Value;
        public IDisplayControl DisplayControl => _displayControl.Value;
        public IChronometer Chronometer => _chronometer.Value;
        public IMessageFactory MessageFactory => _messageFactory.Value;
    }
}
