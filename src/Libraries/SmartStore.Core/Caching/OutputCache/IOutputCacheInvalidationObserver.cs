using System;
using System.Linq.Expressions;
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Caching
{
	public interface IOutputCacheInvalidationObserver
	{
		void ObserveSetting(string settingKey, Action<IOutputCacheProvider> invalidationAction);
		Action<IOutputCacheProvider> GetInvalidationActionForSetting(string settingKey);
	}

	public class NullOutputCacheInvalidationObserver : IOutputCacheInvalidationObserver
	{
		private static readonly IOutputCacheInvalidationObserver _instance = new NullOutputCacheInvalidationObserver();

		public static IOutputCacheInvalidationObserver Instance
		{
			get { return _instance; }
		}

		public Action<IOutputCacheProvider> GetInvalidationActionForSetting(string settingKey)
		{
			return null;
		}

		public void ObserveSetting(string settingKey, Action<IOutputCacheProvider> invalidationAction)
		{
		}
	}

	public static class IOutputCacheInvalidationObserverExtensions
	{
		public static void ObserveSetting<TSetting>(this IOutputCacheInvalidationObserver observer) where TSetting : ISettings
		{
			observer.ObserveSetting<TSetting>(null, null);
		}

		public static void ObserveSetting(this IOutputCacheInvalidationObserver observer, string settingKey)
		{
			observer.ObserveSetting(settingKey, null);
		}

		public static void ObserveSetting<TSetting>(this IOutputCacheInvalidationObserver observer,
			Action<IOutputCacheProvider> invalidationAction) where TSetting : ISettings
		{
			observer.ObserveSetting<TSetting>(null, invalidationAction);
		}

		public static void ObserveSetting<TSetting>(this IOutputCacheInvalidationObserver observer,
			Expression<Func<TSetting, object>> propertyAccessor) where TSetting : ISettings
		{
			observer.ObserveSetting<TSetting>(propertyAccessor, null);
		}

		public static void ObserveSetting<TSetting>(this IOutputCacheInvalidationObserver observer,
			Expression<Func<TSetting, object>> propertyAccessor,
			Action<IOutputCacheProvider> invalidationAction) where TSetting : ISettings
		{
			var groupName = typeof(TSetting).Name;
			var key = string.Empty;

			if (propertyAccessor != null)
			{
				key = groupName + "." + ((MemberExpression)propertyAccessor.Body).Member.Name;
			}
			else
			{
				key = groupName + ".*";
			}

			observer.ObserveSetting(key, invalidationAction);
		}
	}
}
