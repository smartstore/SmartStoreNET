using System;

namespace SmartStore.Core.Domain.Plugins
{
	public class License : BaseEntity
	{
		/// <summary>
		/// The license key
		/// </summary>
		public string LicenseKey { get; set; }

		/// <summary>
		/// The system name
		/// </summary>
		public string SystemName { get; set; }

		/// <summary>
		/// Activation date
		/// </summary>
		public DateTime ActivatedOnUtc { get; set; }

		/// <summary>
		/// The store id
		/// </summary>
		public int StoreId { get; set; }
	}
}
