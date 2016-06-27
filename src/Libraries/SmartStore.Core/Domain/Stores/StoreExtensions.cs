﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace SmartStore.Core.Domain.Stores
{
	public static class StoreExtensions
	{
		/// <summary>
		/// Parse comma-separated Hosts
		/// </summary>
		/// <param name="store">Store</param>
		/// <returns>Comma-separated hosts</returns>
		public static string[] ParseHostValues(this Store store)
		{
			if (store == null)
				throw new ArgumentNullException("store");

			var parsedValues = new List<string>();
			if (!string.IsNullOrEmpty(store.Hosts))
			{
				var hosts = store.Hosts.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				parsedValues.AddRange(hosts.Select(host => host.Trim()).Where(tmp => !string.IsNullOrEmpty(tmp)));
			}

			return parsedValues.ToArray();
		}

		/// <summary>
		/// Indicates whether a store contains a specified host
		/// </summary>
		/// <param name="store">Store</param>
		/// <param name="host">Host</param>
		/// <returns>true - contains, false - no</returns>
		public static bool ContainsHostValue(this Store store, string host)
		{
			if (store == null)
				throw new ArgumentNullException("store");

			if (String.IsNullOrEmpty(host))
				return false;

			var contains = store.ParseHostValues()
								.FirstOrDefault(x => x.Equals(host, StringComparison.InvariantCultureIgnoreCase)) != null;
			return contains;
		}
	}
}
