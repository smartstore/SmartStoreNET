using SmartStore.Core.Configuration;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.GoogleMerchantCenter
{
	public class FroogleSettings : PromotionFeedSettings, ISettings
    {
		public FroogleSettings()
		{
            ProductPictureSize = 125;
			StaticFileName = "google_merchant_center_{0}.xml".FormatWith(CommonHelper.GenerateRandomDigitCode(10));
			Condition = "new";
			OnlineOnly = true;
			AdditionalImages = true;
			SpecialPrice = true;
		}

        public string DefaultGoogleCategory { get; set; }
		public string Condition { get; set; }
		public bool SpecialPrice { get; set; }
		public string Gender { get; set; }
		public string AgeGroup { get; set; }
		public string Color { get; set; }
		public string Size { get; set; }
		public string Material { get; set; }
		public string Pattern { get; set; }
		public bool OnlineOnly { get; set; }
		public int ExpirationDays { get; set; }
		public bool ExportShipping { get; set; }
		public bool ExportBasePrice { get; set; }

		public string AppendDescriptionText1 { get; set; }
		public string AppendDescriptionText2 { get; set; }
		public string AppendDescriptionText3 { get; set; }
		public string AppendDescriptionText4 { get; set; }
		public string AppendDescriptionText5 { get; set; }
    }
}