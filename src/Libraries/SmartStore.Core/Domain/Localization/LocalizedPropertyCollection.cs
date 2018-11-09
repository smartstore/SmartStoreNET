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

		public LocalizedPropertyCollection(string keyGroup, IEnumerable<LocalizedProperty> properties)
		{
			Guard.NotEmpty(keyGroup, nameof(keyGroup));
			Guard.NotNull(properties, nameof(properties));

			_keyGroup = keyGroup;
			_dict = properties.ToDictionarySafe(x => CreateKey(x.LocaleKey, x.EntityId, x.LanguageId), StringComparer.OrdinalIgnoreCase);
		}

		public void MergeWith(LocalizedPropertyCollection other)
		{
			Guard.NotNull(other, nameof(other));

			if (!this._keyGroup.IsCaseInsensitiveEqual(other._keyGroup))
			{
				throw new InvalidOperationException("Expected keygroup '{0}', but was '{1}'".FormatInvariant(this._keyGroup, other._keyGroup));
			}

			other._dict.Merge(this._dict, true);
		}

		public LocalizedProperty Find(int languageId, int entityId, string localeKey)
		{
			return _dict.Get(CreateKey(localeKey, entityId, languageId));
		}

		public string GetValue(int languageId, int entityId, string localeKey)
		{
			if (_dict.TryGetValue(CreateKey(localeKey, entityId, languageId), out var prop))
			{
				return prop.LocaleValue;
			}

			return null;
		}

		private string CreateKey(string localeKey, int entityId, int languageId)
		{
			return string.Concat(localeKey, "-", entityId.ToString(CultureInfo.InvariantCulture), "-", languageId);
		}

		public int Count => _dict.Values.Count;

		public bool IsReadOnly => throw new NotImplementedException();

		public IEnumerator<LocalizedProperty> GetEnumerator() => _dict.Values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _dict.Values.GetEnumerator();
	}
}
