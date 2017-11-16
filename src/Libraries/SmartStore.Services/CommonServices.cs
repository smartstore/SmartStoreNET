using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Events;
using SmartStore.Services.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Security;
using SmartStore.Services.Configuration;
using SmartStore.Services.Stores;
using Autofac;
using SmartStore.Services.Helpers;
using SmartStore.Services.Media;

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
		private readonly Lazy<IPictureService> _pictureService;
		private readonly Lazy<INotifier> _notifier;
		private readonly Lazy<IPermissionService> _permissions;
		private readonly Lazy<ISettingService> _settings;
		private readonly Lazy<IStoreService> _storeService;
		private readonly Lazy<IDateTimeHelper> _dateTimeHelper;
		private readonly Lazy<IDisplayControl> _displayControl;
		private readonly Lazy<IChronometer> _chronometer;

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
			Lazy<IPictureService> pictureService,
			Lazy<INotifier> notifier,
			Lazy<IPermissionService> permissions,
			Lazy<ISettingService> settings,
			Lazy<IStoreService> storeService,
			Lazy<IDateTimeHelper> dateTimeHelper,
			Lazy<IDisplayControl> displayControl,
			Lazy<IChronometer> chronometer)
		{
			this._container = container;
			this._env = env;
			this._cacheManager = cacheManager;
			this._requestCache = requestCache;
			this._dbContext = dbContext;
			this._storeContext = storeContext;
			this._webHelper = webHelper;
			this._workContext = workContext;
			this._eventPublisher = eventPublisher;
			this._localization = localization;
			this._customerActivity = customerActivity;
			this._pictureService = pictureService;
			this._notifier = notifier;
			this._permissions = permissions;
			this._settings = settings;
			this._storeService = storeService;
			this._dateTimeHelper = dateTimeHelper;
			this._displayControl = displayControl;
			this._chronometer = chronometer;
		}

		public IComponentContext Container
		{
			get
			{
				return _container;
			}
		}

		public IApplicationEnvironment ApplicationEnvironment
		{
			get
			{
				return _env.Value;
			}
		}

		public ICacheManager Cache
		{
			get
			{
				return _cacheManager.Value;
			}
		}

		public IRequestCache RequestCache
		{
			get
			{
				return _requestCache.Value;
			}
		}

		public IDbContext DbContext
		{
			get
			{
				return _dbContext.Value;
			}
		}

		public IStoreContext StoreContext
		{
			get
			{
				return _storeContext.Value;
			}
		}

		public IWebHelper WebHelper
		{
			get
			{
				return _webHelper.Value;
			}
		}

		public IWorkContext WorkContext
		{
			get
			{
				return _workContext.Value;
			}
		}

		public IEventPublisher EventPublisher
		{
			get
			{
				return _eventPublisher.Value;
			}
		}

		public ILocalizationService Localization
		{
			get
			{
				return _localization.Value;
			}
		}

		public ICustomerActivityService CustomerActivity
		{
			get
			{
				return _customerActivity.Value;
			}
		}

		public IPictureService PictureService
		{
			get
			{
				return _pictureService.Value;
			}
		}

		public INotifier Notifier
		{
			get
			{
				return _notifier.Value;
			}
		}

		public IPermissionService Permissions
		{
			get 
			{
				return _permissions.Value;
			}
		}

		public ISettingService Settings
		{
			get
			{
				return _settings.Value;
			}
		}


		public IStoreService StoreService
		{
			get
			{
				return _storeService.Value;
			}
		}

		public IDateTimeHelper DateTimeHelper
		{
			get
			{
				return _dateTimeHelper.Value;
			}
		}

		public IDisplayControl DisplayControl
		{
			get
			{
				return _displayControl.Value;
			}
		}

		public IChronometer Chronometer
		{
			get
			{
				return _chronometer.Value;
			}
		}
	}
}
