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
        public int? ItemId { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        public IList<ContentSliderLocalizedModel> Locales { get; set; }

        public IList<SliderSlideModel> Slides { get; set; }

    }

    public class ContentSliderLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.CMS.ContentSlider.Fields.Name")]
        [AllowHtml]
        public string SliderName { get; set; }
    }

	public partial class ContentSliderValidator : AbstractValidator<ContentSliderModel>
	{
		public ContentSliderValidator()
		{
			RuleFor(x => x.SliderName).NotEmpty();
            RuleFor(x => x.SliderType).GreaterThan(-1);
            RuleFor(x => x.Height).Equals(500);
        }
    }
}