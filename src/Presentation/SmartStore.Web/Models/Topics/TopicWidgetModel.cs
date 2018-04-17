using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Topics
{
    public partial class TopicWidgetModel : EntityModelBase
    {
        public string SystemName { get; set; }
        public string Title { get; set; }
		public string Html { get; set; }
		public string TitleTag { get; set; }
		public bool TitleRtl { get; set; }
		public bool HtmlRtl { get; set; }

		public bool WrapContent { get; set; }
        public bool ShowTitle { get; set; }
        public bool IsBordered { get; set; }
    }
}