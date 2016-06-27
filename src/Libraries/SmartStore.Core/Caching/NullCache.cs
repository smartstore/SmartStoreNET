using System;
using SmartStore.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core.Caching
{
    /// <summary>
    /// Represents a null cache
    /// </summary>
    public partial class NullCache : ICacheManager
    {
		private static readonly ICacheManager s_instance = new NullCache();

		public static ICacheManager Instance
		{
			get { return s_instance; }
		}

		public T Get<T>(string key)
		{
			return default(T);
		}

		public T Get<T>(string key, Func<T> acquirer, int? cacheTime = null)
		{
			if (acquirer == null)
			{
				return default(T);
			}
			return acquirer();
		}


		public void Set(string key, object value, int? cacheTime = null)
		{
		}

        public bool Contains(string key)
        {
            return false;
        }

        public void Remove(string key)
        {
        }

		public string[] Keys(string pattern)
		{
			return new string[0];
		}

		public void RemoveByPattern(string pattern)
        {
        }

        public void Clear()
        {
        }
	}
}