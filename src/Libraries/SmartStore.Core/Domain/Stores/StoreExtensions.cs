using System;
using System.Linq;

namespace SmartStore.Core.Domain.Stores
{
    public static class StoreExtensions
    {
        /// <summary>
        /// Indicates whether a store contains a specified host
        /// </summary>
        /// <param name="store">Store</param>
        /// <param name="host">Host</param>
        /// <returns>true - contains, false - no</returns>
        public static bool ContainsHostValue(this Store store, string host)
        {
            Guard.NotNull(store, nameof(store));

            if (string.IsNullOrEmpty(host))
            {
                return false;
            }

            var contains = store.ParseHostValues()
                                .FirstOrDefault(x => x.Equals(host, StringComparison.InvariantCultureIgnoreCase)) != null;
            return contains;
        }

        /// <summary>
        /// Parse comma-separated hosts
        /// </summary>
        /// <param name="store">Store</param>
        /// <returns>Comma-separated hosts</returns>
        public static string[] ParseHostValues(this Store store)
        {
            Guard.NotNull(store, nameof(store));

            if (string.IsNullOrWhiteSpace(store.Hosts))
            {
                return Array.Empty<string>();
            }

            return store.Hosts
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(host => host.Trim())
                .Where(host => !string.IsNullOrWhiteSpace(host))
                .ToArray();
        }
    }
}
