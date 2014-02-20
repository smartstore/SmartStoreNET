using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Services.Events;
using SmartStore.Services.Localization;
using SmartStore.Services.Logging;

namespace SmartStore.Services
{
	public class CommonServices : ICommonServices
	{
		public CommonServices(
			ICacheManager cache,
			IDbContext dbContext,
			IStoreContext storeContext,
			IWebHelper webHelper,
			IWorkContext workContext,
			IEventPublisher eventPublisher,
			ILocalizationService localization,
			ILogger logger,
			ICustomerActivityService customerActivity)
		{
			this.Cache = cache;
			this.DbContext = dbContext;
			this.StoreContext = storeContext;
			this.WebHelper = webHelper;
			this.WorkContext = workContext;
			this.EventPublisher = eventPublisher;
			this.Localization = localization;
			this.Logger = logger;
			this.CustomerActivity = customerActivity;
		}
		
		public ICacheManager Cache
		{
			get;
			private set;
		}

		public IDbContext DbContext
		{
			get;
			private set;
		}

		public IStoreContext StoreContext
		{
			get;
			private set;
		}

		public IWebHelper WebHelper
		{
			get;
			private set;
		}

		public IWorkContext WorkContext
		{
			get;
			private set;
		}

		public IEventPublisher EventPublisher
		{
			get;
			private set;
		}

		public ILocalizationService Localization
		{
			get;
			private set;
		}

		public ILogger Logger
		{
			get;
			private set;
		}

		public ICustomerActivityService CustomerActivity
		{
			get;
			private set;
		}
	}
}
