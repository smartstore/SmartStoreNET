
namespace SmartStore.Core.Plugins
{
	/// <summary>
	/// Marks a plugin as a licensed piece of code where the user has to enter a license key that has to be activated.
	/// </summary>
	public interface ILicensable
	{
		/// <summary>
		/// Whether one license (key) is valid for all stores.
		/// </summary>
		bool OneLicenseForAllStores();
	}


	public enum LicenseStatus : int
	{
		Active = 0,
		Expired,
		Inactive
	}
}
