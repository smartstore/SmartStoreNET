using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Data.Setup
{
    internal class LocaleResourceEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Lang { get; set; }

        public bool UpdateOnly { get; set; }
        public bool Important { get; set; }
    }

    public interface IResourceAddBuilder : IHideObjectMembers
    {
        IResourceAddBuilder Value(string value);
        IResourceAddBuilder Value(string lang, string value);
        IResourceAddBuilder Hint(string value);
        IResourceAddBuilder Hint(string lang, string value);
    }

    public class LocaleResourcesBuilder : IHideObjectMembers
    {
        private readonly List<LocaleResourceEntry> _entries = new List<LocaleResourceEntry>();

        /// <summary>
        /// Deletes one or many locale resources in any language from the database
        /// </summary>
        /// <param name="keys">The key(s) of the resources to delete</param>
        public void Delete(params string[] keys)
        {
            keys.Each(x => _entries.Add(new LocaleResourceEntry { Key = x, Important = true }));
        }

        /// <summary>
        /// Deletes one or many locale resources in the specified language from the database
        /// </summary>
        /// <param name="lang">The language identifier</param>
        /// <param name="keys">The key(s) of the resources to delete</param>
        public void DeleteFor(string lang, params string[] keys)
        {
            Guard.NotEmpty(lang, nameof(lang));
            lang = lang.NullEmpty();

            keys.Each(x => _entries.Add(new LocaleResourceEntry { Key = x, Lang = lang, Important = true }));
        }

        /// <summary>
        /// Updates existing locale resources
        /// </summary>
        /// <param name="key">The key of the resource</param>
        /// <returns>IResourceAddBuilder</returns>
        public IResourceAddBuilder Update(string key)
        {
            Guard.NotEmpty(key, nameof(key));

            Action<string, string, bool> fn = (string v, string l, bool isHint) =>
            {
                string k = key;
                if (isHint)
                {
                    k += ".Hint";
                }

                _entries.Add(new LocaleResourceEntry { Key = k, Value = v, Lang = l, UpdateOnly = true });
            };

            return new ResourceAddBuilder(key, fn);
        }

        /// <summary>
        /// Adds or updates locale resources
        /// </summary>
        /// <param name="key">The key of the resource</param>
        /// <returns>IResourceAddBuilder</returns>
        public IResourceAddBuilder AddOrUpdate(string key)
        {
            Guard.NotEmpty(key, nameof(key));

            Action<string, string, bool> fn = (string v, string l, bool isHint) =>
            {
                string k = key;
                if (isHint)
                {
                    k += ".Hint";
                }

                _entries.Add(new LocaleResourceEntry { Key = k, Value = v, Lang = l });
            };

            return new ResourceAddBuilder(key, fn);
        }

        /// <summary>
        /// Adds or updates locale resources
        /// </summary>
        /// <param name="key">The key of the resource</param>
        /// <param name="value">Primary English (untranslated) value of the resource</param>
        /// <param name="deValue">German value of the resource</param>
        public void AddOrUpdate(string key, string value, string deValue)
        {
            Guard.NotEmpty(key, nameof(key));

            _entries.Add(new LocaleResourceEntry { Key = key, Value = value });
            _entries.Add(new LocaleResourceEntry { Key = key, Value = deValue, Lang = "de" });
        }

        /// <summary>
        /// Adds or updates locale resources
        /// </summary>
        /// <param name="key">The key of the resource</param>
        /// <param name="value">Primary English (untranslated) value of the resource</param>
        /// <param name="deValue">German value of the resource</param>
        /// <param name="hint">Primary English (untranslated) hint resource</param>
        /// <param name="deHint">German hint resource</param>
        public void AddOrUpdate(string key, string value, string deValue, string hint, string deHint)
        {
            Guard.NotEmpty(key, nameof(key));

            AddOrUpdate(key, value, deValue);
            AddOrUpdate(key + ".Hint", hint, deHint);
        }

        internal void Reset()
        {
            _entries.Clear();
        }

        internal IEnumerable<LocaleResourceEntry> Build()
        {
            return _entries.OrderByDescending(x => x.Important).ThenBy(x => x.Lang).ToList();
        }

        #region Nested builder for AddOrUpdate

        internal class ResourceAddBuilder : IResourceAddBuilder
        {
            private readonly Action<string, string, bool> _fn;

            public ResourceAddBuilder(string key, Action<string, string, bool> fn)
            {
                _fn = fn;
            }

            public IResourceAddBuilder Value(string value)
            {
                return Value(null, value);
            }

            public IResourceAddBuilder Value(string lang, string value)
            {
                Guard.NotEmpty(value, nameof(value));
                _fn(value, lang.NullEmpty(), false);
                return this;
            }


            public IResourceAddBuilder Hint(string value)
            {
                return Hint(null, value);
            }

            public IResourceAddBuilder Hint(string lang, string value)
            {
                Guard.NotEmpty(value, nameof(value));
                _fn(value, lang.NullEmpty(), true);
                return this;
            }
        }

        #endregion

    }
}
