using SmartStore.Core.Configuration;
﻿using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace SmartStore.Core.Domain.Cms	
{
    [JsonPersist]
    public class ContentSliderSettings : ISettings
    {
        public ContentSliderSettings() 
        {
            Slides = new List<ContentSliderSlideSettings>();
			IsActive = true;
			ContentSliderHeight = "300px";
        }

        public bool IsActive { get; set; }

        public string ContentSliderHeight { get; set; }

        public int BackgroundPictureId { get; set; }

        [JsonIgnore]
        public string BackgroundPictureUrl { get; set; }

        public bool AutoPlay { get; set; }

        public int AutoPlayDelay { get; set; }

        public IList<ContentSliderSlideSettings> Slides { get; set; }
    }

    public class ContentSliderSlideSettings : ISettings
    {
        public int DisplayOrder { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public int PictureId { get; set; }

        [JsonIgnore]
        public string PictureUrl { get; set; }

        [JsonIgnore]
        public string LanguageName { get; set; }

        public string LanguageCulture { get; set; }

        public bool Published { get; set; }

		public bool LimitedToStores { get; set; }
		public int[] SelectedStoreIds { get; set; }

        public ContentSliderButtonSettings Button1 { get; set; }

        public ContentSliderButtonSettings Button2 { get; set; }

        public ContentSliderButtonSettings Button3 { get; set; }
    }

    public class ContentSliderButtonSettings : ISettings
    {
        public string Text { get; set; }

        public string Type { get; set; }

        public string Url { get; set; }

        public bool Published { get; set; }
    }
}