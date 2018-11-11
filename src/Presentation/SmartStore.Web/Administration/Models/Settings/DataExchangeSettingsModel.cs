using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
	public partial class DataExchangeSettingsModel
	{
		#region General

		[SmartResourceDisplayName("Admin.Configuration.Settings.DataExchange.MaxFileNameLength")]
		public int MaxFileNameLength { get; set; }

		#endregion

		#region Import

		[SmartResourceDisplayName("Admin.Configuration.Settings.DataExchange.ImageImportFolder")]
		public string ImageImportFolder { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.DataExchange.ImageDownloadTimeout")]
		public int ImageDownloadTimeout { get; set; }

		#endregion

		#region Export

		#endregion
	}
}