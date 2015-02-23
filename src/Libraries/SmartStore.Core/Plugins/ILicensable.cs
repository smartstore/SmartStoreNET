
namespace SmartStore.Core.Plugins
{
	/// <summary>
	/// Marks a plugin as a licensed piece of code where the user has to enter a license key that has to be activated.
	/// Note that a license key is only valid for one IP address (the one used to activate the key).
	/// </summary>
	public interface ILicensable
	{
		/// <summary>
		/// Whether one license (key) is valid for all stores. Otherwise a new key is required for each store.
		/// </summary>
		bool HasSingleLicenseForAllStores { get; }

		/// <summary>
		/// Whether one license (key) is valid for all plugin versions. Otherwise a new key is required if the major version changes.
		/// </summary>
		bool HasSingleLicenseForAllVersions { get; }
	}
}
