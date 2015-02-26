
namespace SmartStore.Core.Plugins
{
	/// <summary>
	/// Marks a plugin as a licensed piece of code where the user has to enter a license key that has to be activated.
	/// Note that a license key is only valid for the IP address that activated the key.
	/// </summary>
	public interface ILicensable
	{
		/// <summary>
		/// Whether one license (key) is valid for all stores. Otherwise a new key is required for each store.
		/// </summary>
		bool HasSingleLicenseForAllStores { get; }
	}
}
