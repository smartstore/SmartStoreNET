using System;
using System.Collections.Generic;

namespace SmartStore.Web.Models.Common
{
    public class MetaPropertiesModel
    {
        public string Site { get; set; }
        public string SiteName { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string ImageUrl { get; set; }
        public string ImageAlt { get; set; }
        public string ImageType { get; set; }
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }

        public DateTime PublishedTime { get; set; }
        public string ArticleSection { get; set; }
        public IEnumerable<string> ArticleTags { get; set; }

        public string TwitterSite { get; set; }
        public string FacebookAppId { get; set; }
    }
}