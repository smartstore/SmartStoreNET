using SmartStore.Core.Domain.ContentSlider;
using SmartStore.Services;
using SmartStore.Services.ContentSlider;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Models.ContentSlider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Controllers
{
    public class ContentSliderController : PublicControllerBase
    {
        private readonly ICommonServices _services;
        private readonly IContentSliderService _contentSliderService;
        private readonly ContentSliderHelper _helper;

        public ContentSliderController(ICommonServices services,
            IContentSliderService contentSliderService,
            ContentSliderHelper helper)
        {
            _services = services;
            _contentSliderService = contentSliderService;
            _helper = helper;
        }

        [ChildActionOnly]
        public ActionResult HomePageContentSlider()
        {
            var contentSliders = _contentSliderService.GetContentSliderByType((int)SliderType.HomePageSlider);
            ContentSliderModel CSModel = new ContentSliderModel();
            ContentSlider slider = new ContentSlider();
            if (contentSliders.Count > 0)
                CSModel = MapSliderObject(slider, contentSliders);

            return PartialView("ContentSlider", CSModel);
        }

        [ChildActionOnly]
        public ActionResult CategoryContentSlider(int CategoryId)
        {
            ContentSliderModel CSModel = new ContentSliderModel();
            ContentSlider slider = new ContentSlider();

            var ThisCategoryContentSliders = _contentSliderService.GetContentSliderByTypeAndItemId((int)SliderType.CategorySlider, CategoryId);
            if (ThisCategoryContentSliders.Count > 0)
                CSModel = MapSliderObject(slider, ThisCategoryContentSliders);
            else
            {
                var contentSliders = _contentSliderService.GetContentSliderByType((int)SliderType.CategorySlider);
                if (contentSliders.Count > 0)
                    CSModel = MapSliderObject(slider, contentSliders);
            }

            return PartialView("ContentSlider", CSModel);
        }

        [ChildActionOnly]
        public ActionResult ManufacturerContentSlider(int ManufacturerId)
        {
            ContentSliderModel CSModel = new ContentSliderModel();
            ContentSlider slider = new ContentSlider();

            var ThisManufacturerContentSliders = _contentSliderService.GetContentSliderByTypeAndItemId((int)SliderType.BrandSlider, ManufacturerId);
            if (ThisManufacturerContentSliders.Count > 0)
                CSModel = MapSliderObject(slider, ThisManufacturerContentSliders);
            else
            {
                var contentSliders = _contentSliderService.GetContentSliderByType((int)SliderType.BrandSlider);
                if (contentSliders.Count > 0)
                    CSModel = MapSliderObject(slider, contentSliders);
            }

            return PartialView("ContentSlider", CSModel);
        }

        private ContentSliderModel MapSliderObject(ContentSlider slider, IList<ContentSlider> contentSliders)
        {
            ContentSliderModel CSModel;
            slider = contentSliders[0];

            CSModel = new ContentSliderModel
            {
                SliderId = slider.Id,
                IsActive = slider.IsActive,
                RandamizeSlides = slider.RandamizeSlides,
                AutoPlay = slider.AutoPlay,
                Delay = slider.Delay,
                Height = slider.Height
            };

            IList<Slide> Slides = _contentSliderService.GetContentSliderSlidesBySliderId(CSModel.SliderId);

            List<SlideModel> SliderSlides = new List<SlideModel>();
            foreach (var slide in Slides)
            {
                var slideModelObject = new SlideModel
                {
                    SlideId = slide.Id,
                    SlideTitle = slide.SlideTitle,
                    SlideContent = slide.SlideContent,
                    Picture = slide.Picture,
                    PictureId = slide.PictureId,
                    IsActive = slide.IsActive,
                    DisplayButton = slide.DisplayButton,
                    DisplayOrder = slide.DisplayOrder,
                    DisplayPrice = slide.DisplayPrice,
                    SliderId = CSModel.SliderId,
                    SlideType = slide.SlideType,
                    ItemId = slide.ItemId
                };

                _helper.PrepareContentSliderModel(slideModelObject);
                SliderSlides.Add(slideModelObject);
            }

            if (SliderSlides.Count > 0)
                CSModel.Slides = SliderSlides;
            else
                CSModel = new ContentSliderModel();

            return CSModel;
        }
    }
}