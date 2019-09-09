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

namespace SmartStore.Admin.Models.Catalog
{
	[Validator(typeof(ContentSliderValidator))]
    public class ContentSliderModel : TabbableModel, ILocalizedModel<ContentSliderLocalizedModel>
	{
        public ContentSliderModel()
        {
            Locales = new List<ContentSliderLocalizedModel>();
        }

		[SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.Name")]
        [AllowHtml]
        public string SliderName { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.IsActive")]
        [AllowHtml]
        public bool IsActive { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.Height")]
        [AllowHtml]
        public int Height { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.RandamizeSlides")]
        [AllowHtml]
        public bool RandamizeSlides { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.AutoPlay")]
        [AllowHtml]
        public bool AutoPlay { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.Delay")]
        [AllowHtml]
        public int Delay { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.SliderType")]
        [AllowHtml]
        public int SliderType { get; set; }

        //[SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.SliderType")]
        //[AllowHtml]
        //public string SliderTypeName { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.ItemId")]
        [AllowHtml]
        public int ItemId { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        public IList<ContentSliderLocalizedModel> Locales { get; set; }

        public IList<SliderSlidModel> Slides { get; set; }

		#region Nested classes

		public class SliderSlidModel : EntityModelBase
        {
            public int SliderId { get; set; }

            public int SlideId { get; set; }

            public int? ItemId { get; set; }

            [SmartResourceDisplayName("Admin.CMS.ContentSlider.Slides.Fields.Slide")]
            public string SlideTitle { get; set; }

			[SmartResourceDisplayName("Admin.CMS.ContentSlider.Slides.Fields.Content")]
			public string SlideContent { get; set; }

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
            public int DisplayOrder1 { get; set; }
        }

        #endregion
    }

    public class ContentSliderLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.Name")]
        [AllowHtml]
        public string SliderName { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.Description")]
        [AllowHtml]
        public string Description {get;set;}

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }
    }

	public partial class ContentSliderValidator : AbstractValidator<ContentSliderModel>
	{
		public ContentSliderValidator()
		{
			RuleFor(x => x.SliderName).NotEmpty();
		}
	}
}