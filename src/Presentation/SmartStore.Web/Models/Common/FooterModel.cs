using SmartStore.Web.Framework.Mvc;
using System.Collections.Generic;

namespace SmartStore.Web.Models.Common
{
    public partial class FooterModel : ModelBase
    {
        public FooterModel()
        {
            Topics = new Dictionary<string, string>();
        }

        public string StoreName { get; set; }

        public string LegalInfo { get; set; }
        public bool ShowLegalInfo { get; set; }
        public bool ShowThemeSelector { get; set; }
        public string NewsletterEmail { get; set; }
		public string SmartStoreHint { get; set; }
        public bool HideNewsletterBlock { get; set; }
        public bool BlogEnabled { get; set; }
        public bool ForumEnabled { get; set; }
        public Dictionary<string, string> Topics { get; set; }

        public bool ShowSocialLinks { get; set; }
        public string FacebookLink { get; set; }
        public string GooglePlusLink { get; set; }
        public string TwitterLink { get; set; }
        public string PinterestLink { get; set; }
        public string YoutubeLink { get; set; }
    }
}