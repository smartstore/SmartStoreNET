using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SmartStore.Core;
using SmartStore.Core.Configuration;

namespace SmartStore.Data.Setup
{
    public static class SeedEntityExtensions
    {
        #region fluent extensions

        public static SeedEntityAlterer<T, TKey> WithKey<T, TKey>(this IList<T> list, Expression<Func<T, TKey>> expression) where T : BaseEntity
        {
            return new SeedEntityAlterer<T, TKey>(list, expression);
        }

        public static SeedSettingsAlterer Alter<TSettings>(this IList<ISettings> list, Action<TSettings> action) where TSettings : class, ISettings, new()
        {
            var alterer = new SeedSettingsAlterer(list);
            return alterer.Alter<TSettings>(action);
        }

        #endregion

        #region fluent classes

        public class SeedSettingsAlterer
        {
            private readonly Dictionary<Type, ISettings> _settingsMap; // for faster access!

            public SeedSettingsAlterer(IList<ISettings> settings)
            {
                // fetch all types from list and build a key/value map for faster access.
                _settingsMap = new Dictionary<Type, ISettings>(settings.Count);

                foreach (var setting in settings)
                {
                    _settingsMap.Add(setting.GetType(), setting);
                }
            }

            public SeedSettingsAlterer Alter<TSettings>(Action<TSettings> action) where TSettings : class, ISettings, new()
            {
                ISettings setting = null;
                if (_settingsMap.TryGetValue(typeof(TSettings), out setting))
                {
                    action(setting as TSettings);
                }

                return this;
            }
        }


        public class SeedEntityAlterer<T, TKey> where T : BaseEntity
        {
            private readonly Dictionary<TKey, T> _entityMap; // for faster access!
            private readonly IList<T> _entities;

            public SeedEntityAlterer(IList<T> list, Expression<Func<T, TKey>> keyExpression)
            {
                _entities = list;

                // fetch all key values from list and build a key/value map for faster access.
                _entityMap = new Dictionary<TKey, T>(list.Count);
                var fn = keyExpression.Compile();

                foreach (var entity in list)
                {
                    TKey key = fn.Invoke(entity);
                    _entityMap.Add(key, entity);
                }
                
            }

            public SeedEntityAlterer<T, TKey> Alter(TKey key, Action<T> action)
            {
                T entity = default(T);
                if (_entityMap.TryGetValue(key, out entity)) 
                {
                    action(entity);
                }

                return this;
            }

            public SeedEntityAlterer<T, TKey> Remove(TKey key)
            {
                T entity = default(T);
                if (_entityMap.TryGetValue(key, out entity))
                {
                    _entityMap.Remove(key);
                    _entities.Remove(entity);
                }

                return this;
            }
        }

        #endregion
    }
}
