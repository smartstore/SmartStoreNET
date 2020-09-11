using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace SmartStore.Core.Domain.Localization
{
    public class LocalizedPropertyCollection : IReadOnlyCollection<LocalizedProperty>
    {
        private readonly string _keyGroup;
        private readonly IDictionary<string, LocalizedProperty> _dict;
        private HashSet<int> _requestedSet;

        public LocalizedPropertyCollection(string keyGroup, int[] requestedSet, IEnumerable<LocalizedProperty> items)
        {
            Guard.NotEmpty(keyGroup, nameof(keyGroup));
            Guard.NotNull(items, nameof(items));

            _keyGroup = keyGroup;
            _dict = items.ToDictionarySafe(x => CreateKey(x.LocaleKey, x.EntityId, x.LanguageId), StringComparer.OrdinalIgnoreCase);

            if (requestedSet != null && requestedSet.Length > 0)
            {
                _requestedSet = new HashSet<int>(requestedSet);
            }
        }

        public void MergeWith(LocalizedPropertyCollection other)
        {
            Guard.NotNull(other, nameof(other));

            if (!this._keyGroup.IsCaseInsensitiveEqual(other._keyGroup))
            {
                throw new InvalidOperationException("Expected keygroup '{0}', but was '{1}'".FormatInvariant(this._keyGroup, other._keyGroup));
            }

            // Merge dictionary
            other._dict.Merge(this._dict, true);

            // Merge requested set (entity ids)
            if (this._requestedSet != null)
            {
                if (other._requestedSet == null)
                {
                    other._requestedSet = new HashSet<int>(this._requestedSet);
                }
                else
                {
                    other._requestedSet.AddRange(this._requestedSet);
                }
            }
        }

        public string GetValue(int languageId, int entityId, string localeKey)
        {
            return Find(languageId, entityId, localeKey)?.LocaleValue;
        }

        public LocalizedProperty Find(int languageId, int entityId, string localeKey)
        {
            var item = _dict.Get(CreateKey(localeKey, entityId, languageId));

            if (item == null && (_requestedSet == null || _requestedSet.Contains(entityId)))
            {
                // Although the item does not exist in the local dictionary it has been requested
                // from the database, which means it does not exist in the db either.
                // Avoid the upcoming roundtrip.
                return new LocalizedProperty
                {
                    LocaleKeyGroup = _keyGroup,
                    EntityId = entityId,
                    LanguageId = languageId,
                    LocaleKey = localeKey
                };
            }

            return item;
        }

        private string CreateKey(string localeKey, int entityId, int languageId)
        {
            return string.Concat(localeKey, "-", entityId.ToString(CultureInfo.InvariantCulture), "-", languageId);
        }

        public int Count => _dict.Values.Count;
        public IEnumerator<LocalizedProperty> GetEnumerator()
        {
            return _dict.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.Values.GetEnumerator();
        }
    }
}
