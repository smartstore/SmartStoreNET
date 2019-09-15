using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.ContentSlider
{
	[Validator(typeof(SliderSlideValidator))]
    public class SliderSlideModel : TabbableModel, ILocalizedModel<SliderSlidLocalizedModel>
    {
        public SliderSlideModel()
        {
            Locales = new List<SliderSlidLocalizedModel>();

        }
        public int SliderId { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slides.Fields.SlideId")]
        public int SlideId { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slides.Fields.ItemId")]
        public int? ItemId { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slides.Fields.SlideTitle")]
        public string SlideTitle { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slides.Fields.Content")]
        public string SlideContent { get; set; }

        [UIHint("Picture")]
        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slides.Fields.Picture")]
        public int? PictureId { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slides.Fields.SlideType")]
        public int SlideType { get; set; }

        //         [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slides.Fields.SlideTypeName")]
        //public string SlideTypeName { get; set; }
        public string SlideTypeLabelHint { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slides.Fields.IsActive")]
        public bool IsActive { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slides.Fields.DisplayPrice")]
        public bool DisplayPrice { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slides.Fields.DisplayButton")]
        public bool DisplayButton { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        //we don't name it DisplayOrder because Telerik has a small bug 
        //"if we have one more editor with the same name on a page, it doesn't allow editing"
        //in our case it's category.DisplayOrder
        public int DisplayOrder { get; set; }

        public IList<SliderSlidLocalizedModel> Locales { get; set; }
    }

    public class SliderSlidLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slide.Fields.SlideTitle")]
        [AllowHtml]
        public string SlideTitle { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slide.Fields.SlideContent")]
        [AllowHtml]
        public string SlideContent { get; set; }
    }

    public partial class SliderSlideValidator : AbstractValidator<SliderSlideModel>
    {
        public SliderSlideValidator()
        {
            //RuleFor(x => x.SlideTitle).NotEmpty();
            //RuleFor(x => x.SlideContent).NotEmpty();
            RuleFor(x => x.SlideType).GreaterThan(-1);
        }
    }
}