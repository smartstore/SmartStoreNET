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

		public void Delete(params string[] keys)
		{
			keys.Each(x => _entries.Add(new LocaleResourceEntry { Key = x, Important = true }));
		}

		public void DeleteFor(string lang, params string[] keys)
		{
			Guard.ArgumentNotEmpty(() => lang);
			keys.Each(x => _entries.Add(new LocaleResourceEntry { Key = x, Lang = lang, Important = true }));
		}

		public IResourceAddBuilder AddOrUpdate(string key)
		{
			Guard.ArgumentNotEmpty(() => key);

			Action<string, string> fn = (string v, string l) => 
			{
				_entries.Add(new LocaleResourceEntry { Key = key, Value = v, Lang = l });
			};

			return new ResourceAddBuilder(key, fn);
		}

		internal IEnumerable<LocaleResourceEntry> Build()
		{
			return _entries.OrderByDescending(x => x.Important).ThenBy(x => x.Lang);
		}

		#region Nested classes

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
				_fn(value, lang);
				return this;
			}
		}

		#endregion

	}
}
