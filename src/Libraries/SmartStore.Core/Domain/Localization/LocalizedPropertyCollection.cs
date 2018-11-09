using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace SmartStore.Core.Domain.Localization
{
	public class LocalizedPropertyCollection : IReadOnlyCollection<LocalizedProperty>
	{
		private IDictionary<string, LocalizedProperty> _dict;

		public LocalizedPropertyCollection(IEnumerable<LocalizedProperty> properties)
		{
			Guard.NotNull(properties, nameof(properties));

			_dict = properties.ToDictionarySafe(x => CreateKey(x.LocaleKey, x.EntityId, x.LanguageId), StringComparer.OrdinalIgnoreCase);
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
		public IEnumerator<LocalizedProperty> GetEnumerator() => _dict.Values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _dict.Values.GetEnumerator();
	}
}
