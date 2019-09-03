using SmartStore.Core.Domain.Media;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.Web.Models.ContentSlider
{
    public enum SlideType { NormalSlide, ProductSlide, CategorySlide, BrandSlide };
    public enum SliderType { HomePageSlider, CategorySlider, BrandSlider };

    public class ContentSliderModel : EntityModelBase
    {
        public int SliderId {get;set;}
        public bool IsActive { get; set; }
		public bool RandamizeSlides { get; set; }
		public bool AutoPlay { get; set; }
        public int Delay { get; set; }
		public int Height { get; set; }
        public IList<SlideModel> Slides { get; set; }
    }

    public class SlideModel
    {
        public int SlideId { get; set; }
        public bool IsActive { get; set; }
        public int SliderId { get; set; }
        public string SlideTitle { get; set; }
        public string SlideContent { get; set; }
        public int PictureId { get; set; }
        public int SlideType { get; set; }
        public int DisplayOrder { get; set; }
        public bool DisplayPrice { get; set; }
        public bool DisplayButton { get; set; }
        public int ItemId { get; set; }
        public Picture Picture { get; set; }
        public ContentSliderSlidePictureModel PictureModel { get; set; }
        public ProductDetailsModel ProductDetails { get; set; }

    }

    public class ContentSliderSlidePictureModel : ModelBase
    {
        public ContentSliderSlidePictureModel()
        {
        }

        public string Name { get; set; }
        public string AlternateText { get; set; }
        public bool DefaultPictureZoomEnabled { get; set; }
        public string PictureZoomType { get; set; }
        public PictureModel DefaultPictureModel { get; set; }
        public PictureModel PictureModel { get; set; }
        public int GalleryStartIndex { get; set; }
    }
}