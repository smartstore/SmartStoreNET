using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core.Configuration;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Utilities;

namespace SmartStore.Core.Caching
{
    public class ObserveEntityContext
    {
        public IOutputCacheProvider OutputCacheProvider { get; set; }
        public IDisplayControl DisplayControl { get; set; }
        public BaseEntity Entity { get; set; }
        public IHookedEntity EntityEntry { get; set; }
        public bool Handled { get; set; }
        public ContainerManager ServiceContainer { get; set; }
    }

    /// <summary>
    /// Allows registration of output cache invalidation handlers
    /// </summary>
    public interface IOutputCacheInvalidationObserver
    {
        /// <summary>
        /// Registers an entity observer. The passed observer is responsible for invalidating the output cache
        /// by calling one of the invalidation methods in the <see cref="IOutputCacheProvider"/> instance.
        /// The observer must then set the <see cref="ObserveEntityContext.Handled"/> property to <c>true</c>
        /// to signal the framework that it should skip executing subsequent observers. 
        /// </summary>
        /// <param name="observer">The observer action</param>
        /// <remarks>
        /// The implementation of this interface is singleton scoped.
        /// Don't use objects with shorter lifetime in your handler as this will lead to memory leaks.
        /// If your handler needs to call service methods, resolve required services
        /// with <see cref="ObserveEntityContext.ServiceContainer"/>.
        /// </remarks>
        void ObserveEntity(Action<ObserveEntityContext> observer);

        IEnumerable<Action<ObserveEntityContext>> GetEntityObservers();

        /// <summary>
        /// Registers a setting key to be observed by the framework. If the value for the passed
        /// setting key changes, the framework calls the <paramref name="invalidationAction"/> handler.
        /// The key can either be fully qualified - e.g. "CatalogSettings.ShowProductSku" -,
        /// or prefixed - e.g. "CatalogSettings.*". The latter calls the invalidator when ANY CatalogSetting changes.
        /// </summary>
        /// <param name="invalidationAction">
        /// The invalidation action handler. If <c>null</c> is passed, the framework
        /// uses the default invalidator, which is <see cref="IOutputCacheProvider.RemoveAll()"/>.
        /// </param>
        void ObserveSetting(string settingKey, Action<IOutputCacheProvider> invalidationAction);

        Action<IOutputCacheProvider> GetInvalidationActionForSetting(string settingKey);
    }

    public class NullOutputCacheInvalidationObserver : IOutputCacheInvalidationObserver
    {
        private static readonly IOutputCacheInvalidationObserver _instance = new NullOutputCacheInvalidationObserver();

        public static IOutputCacheInvalidationObserver Instance => _instance;

        public void ObserveEntity(Action<ObserveEntityContext> observer)
        {
        }

        public IEnumerable<Action<ObserveEntityContext>> GetEntityObservers()
        {
            return Enumerable.Empty<Action<ObserveEntityContext>>();
        }

        public void ObserveSetting(string settingKey, Action<IOutputCacheProvider> invalidationAction)
        {
        }

        public Action<IOutputCacheProvider> GetInvalidationActionForSetting(string settingKey)
        {
            return null;
        }
    }

    public static class IOutputCacheInvalidationObserverExtensions
    {
        public static void ObserveSetting(this IOutputCacheInvalidationObserver observer, string settingKey)
        {
            observer.ObserveSetting(settingKey, null);
        }

        /// <summary>
        /// Registers a concrete setting class to be observed by the framework. If any setting property
        /// of <typeparamref name="TSetting"/> changes, the framework will purge the cache.
        /// </summary>
        /// <typeparam name="TSetting">The type of the concrete setting class to observe</typeparam>
        /// <remarks>
        /// A property observer precedes a class observer.
        /// </remarks>
        public static void ObserveSettings<TSetting>(this IOutputCacheInvalidationObserver observer)
            where TSetting : ISettings
        {
            ObserveSettings<TSetting>(observer, null);
        }

        /// <summary>
        /// Registers a concrete setting class to be observed by the framework. If any setting property
        /// of <typeparamref name="TSetting"/> changes, the framework will call the <paramref name="invalidationAction"/> handler.
        /// </summary>
        /// <typeparam name="TSetting">The type of the concrete setting class to observe</typeparam>
        /// <param name="invalidationAction">
        /// The invalidation action handler. If <c>null</c> is passed, the framework
        /// uses the default invalidator, which is <see cref="IOutputCacheProvider.RemoveAll()"/>.
        /// </param>
        /// <remarks>
        /// A property observer precedes a class observer.
        /// </remarks>
        public static void ObserveSettings<TSetting>(
            this IOutputCacheInvalidationObserver observer,
            Action<IOutputCacheProvider> invalidationAction) where TSetting : ISettings
        {
            var key = typeof(TSetting).Name + ".*";
            observer.ObserveSetting(key, invalidationAction);
        }

        /// <summary>
        /// Registers a setting property to be observed by the framework. If the value for the passed
        /// property changes, the framework will purge the cache.
        /// </summary>
        /// <typeparam name="TSetting">The type of the concrete setting class which contains the property</typeparam>
        /// <param name="propertyAccessor">The property lambda</param>
        public static void ObserveSettingProperty<TSetting>(
            this IOutputCacheInvalidationObserver observer,
            Expression<Func<TSetting, object>> propertyAccessor) where TSetting : ISettings
        {
            ObserveSettingProperty<TSetting>(observer, propertyAccessor, null);
        }

        /// <summary>
        /// Registers a setting property to be observed by the framework. If the value for the passed
        /// property changes, the framework will call the <paramref name="invalidationAction"/> handler.
        /// </summary>
        /// <typeparam name="TSetting">The type of the concrete setting class which contains the property</typeparam>
        /// <param name="propertyAccessor">The property lambda</param>
        /// <param name="invalidationAction">
        /// The invalidation action handler. If <c>null</c> is passed, the framework
        /// uses the default invalidator, which is <see cref="IOutputCacheProvider.RemoveAll()"/>.
        /// </param>
        public static void ObserveSettingProperty<TSetting>(
            this IOutputCacheInvalidationObserver observer,
            Expression<Func<TSetting, object>> propertyAccessor,
            Action<IOutputCacheProvider> invalidationAction) where TSetting : ISettings
        {
            Guard.NotNull(propertyAccessor, nameof(propertyAccessor));

            var key = TypeHelper.NameOf<TSetting>(propertyAccessor, true);
            observer.ObserveSetting(key, invalidationAction);
        }
    }
}
