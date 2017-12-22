using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Events;
using SmartStore.Services.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Security;
using SmartStore.Services.Configuration;
using SmartStore.Services.Stores;
using SmartStore.Services.Helpers;
using Autofac;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;

namespace SmartStore.Services
{	
	public interface ICommonServices
	{
		IComponentContext Container { get; }
		IApplicationEnvironment ApplicationEnvironment { get; }
		ICacheManager Cache { get; }
		IRequestCache RequestCache { get; }
		IDisplayControl DisplayControl { get; }
		IDbContext DbContext { get; }
		IStoreContext StoreContext { get; }
		IWebHelper WebHelper { get; }
		IWorkContext WorkContext { get; }
		IEventPublisher EventPublisher { get; }
		ILocalizationService Localization { get; }
		ICustomerActivityService CustomerActivity { get; }
		IPictureService PictureService { get; }
		INotifier Notifier { get; }
		IPermissionService Permissions { get; }
		ISettingService Settings { get; }
		IStoreService StoreService { get; }
		IDateTimeHelper DateTimeHelper { get; }
		IChronometer Chronometer { get; }
		IMessageFactory MessageFactory { get; }
	}

	public static class ICommonServicesExtensions
	{
		public static TService Resolve<TService>(this ICommonServices services)
		{
			return services.Container.Resolve<TService>();
		}

		public static TService Resolve<TService>(this ICommonServices services, object serviceKey)
		{
			return services.Container.ResolveKeyed<TService>(serviceKey);
		}

		public static TService ResolveNamed<TService>(this ICommonServices services, string serviceName)
		{
			return services.Container.ResolveNamed<TService>(serviceName);
		}

		public static object Resolve(this ICommonServices services, Type serviceType)
		{
			return services.Resolve(null, serviceType);
		}

		public static object Resolve(this ICommonServices services, object serviceKey, Type serviceType)
		{
			return services.Container.ResolveKeyed(serviceKey, serviceType);
		}

		public static object ResolveNamed(this ICommonServices services, string serviceName, Type serviceType)
		{
			return services.Container.ResolveNamed(serviceName, serviceType);
		}
	}
}
