using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Topics
{
    public partial class TopicModel : EntityModelBase
    {
        public string SystemName { get; set; }

        public bool IncludeInSitemap { get; set; }

        public bool IsPasswordProtected { get; set; }

        public LocalizedValue<string> Title { get; set; }

        public LocalizedValue<string> Body { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

		public string SeName { get; set; }

		public string TitleTag { get; set; }

		public bool RenderAsWidget { get; set; }
	}
}