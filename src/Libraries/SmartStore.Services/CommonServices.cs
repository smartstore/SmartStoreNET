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

namespace SmartStore.Services
{
	public class CommonServices : ICommonServices
	{
		private readonly Lazy<ICacheManager> _cache;
		private readonly Lazy<IDbContext> _dbContext;
		private readonly Lazy<IStoreContext> _storeContext;
		private readonly Lazy<IWebHelper> _webHelper;
		private readonly Lazy<IWorkContext> _workContext;
		private readonly Lazy<IEventPublisher> _eventPublisher;
		private readonly Lazy<ILocalizationService> _localization;
		private readonly Lazy<ICustomerActivityService> _customerActivity;
		private readonly Lazy<INotifier> _notifier;
		private readonly Lazy<IPermissionService> _permissions;
		private readonly Lazy<ISettingService> _settings;
		private readonly Lazy<IStoreService> _storeService;
		
		public CommonServices(
			Func<string, Lazy<ICacheManager>> cache,
			Lazy<IDbContext> dbContext,
			Lazy<IStoreContext> storeContext,
			Lazy<IWebHelper> webHelper,
			Lazy<IWorkContext> workContext,
			Lazy<IEventPublisher> eventPublisher,
			Lazy<ILocalizationService> localization,
			Lazy<ICustomerActivityService> customerActivity,
			Lazy<INotifier> notifier,
			Lazy<IPermissionService> permissions,
			Lazy<ISettingService> settings,
			Lazy<IStoreService> storeService)
		{
			this._cache = cache("static");
			this._dbContext = dbContext;
			this._storeContext = storeContext;
			this._webHelper = webHelper;
			this._workContext = workContext;
			this._eventPublisher = eventPublisher;
			this._localization = localization;
			this._customerActivity = customerActivity;
			this._notifier = notifier;
			this._permissions = permissions;
			this._settings = settings;
			this._storeService = storeService;
		}
		
		public ICacheManager Cache
		{
			get
			{
				return _cache.Value;
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
	}
}
