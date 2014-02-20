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
	
	public interface ICommonServices
	{
		ICacheManager Cache 
		{ 
			get;
		}

		IDbContext DbContext
		{
			get;
		}

		IStoreContext StoreContext
		{
			get;
		}

		IWebHelper WebHelper
		{
			get;
		}

		IWorkContext WorkContext
		{
			get;
		}

		IEventPublisher EventPublisher
		{
			get;
		}

		ILocalizationService Localization
		{
			get;
		}

		ILogger Logger
		{
			get;
		}

		ICustomerActivityService CustomerActivity
		{
			get;
		}
	}

}
