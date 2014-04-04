using SmartStore.Core.Configuration;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Plugin.Feed.Froogle
{
	public class FroogleSettings : PromotionFeedSettings, ISettings
    {
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