using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore.Data.Setup
{

	internal class LocaleResourceEntry
	{
		public string Key { get; set; }
		public string Value { get; set; }
		public string Lang { get; set; }

		public bool Important { get; set; }
	}

	public interface IResourceAddBuilder : IHideObjectMembers
	{
		IResourceAddBuilder Value(string value);
		IResourceAddBuilder Value(string lang, string value);
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
			Guard.ArgumentNotEmpty(() => lang);
			lang = lang.NullEmpty();

			keys.Each(x => _entries.Add(new LocaleResourceEntry { Key = x, Lang = lang, Important = true }));
		}

		/// <summary>
		/// Adds or updates locale resources
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public IResourceAddBuilder AddOrUpdate(string key)
		{
			Guard.ArgumentNotEmpty(() => key);

			Action<string, string> fn = (string v, string l) => 
			{
				_entries.Add(new LocaleResourceEntry { Key = key, Value = v, Lang = l });
			};

			return new ResourceAddBuilder(key, fn);
		}

		internal void Reset()
		{
			_entries.Clear();
		}

		internal IEnumerable<LocaleResourceEntry> Build()
		{
			return _entries.OrderByDescending(x => x.Important).ThenBy(x => x.Lang);
		}

		#region Nested builder for AddOrUpdate

		internal class ResourceAddBuilder : IResourceAddBuilder
		{
			private readonly Action<string, string> _fn;

			public ResourceAddBuilder(string key, Action<string, string> fn)
			{
				_fn = fn;
			}
			
			public IResourceAddBuilder Value(string value)
			{
				return Value(null, value);
			}

			public IResourceAddBuilder Value(string lang, string value)
			{
				Guard.ArgumentNotEmpty(() => value);
				_fn(value, lang.NullEmpty());
				return this;
			}
		}

		#endregion

	}
}
