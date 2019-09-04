using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Localization;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Media;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.ContentSlider;
using SmartStore.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.Web.Controllers
{
    public class ContentSliderHelper
    {
        private readonly ICommonServices _services;
        private readonly CatalogHelper _helper;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;
        public Localizer T { get; set; }

        public ContentSliderHelper(
            ICommonServices services,
            CatalogHelper helper,
            MediaSettings mediaSettings,
            IPictureService pictureService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService)
        {
            _services = services;
            _helper = helper;
            _productService = productService;
            _categoryService = categoryService;
            _mediaSettings = mediaSettings;
            T = NullLocalizer.Instance;
            _pictureService = pictureService;
            _manufacturerService = manufacturerService;
        }

        public void PrepareContentSliderModel(SlideModel slide)
        {
            if (slide.SlideType == (int)SlideType.NormalSlide)
            {
                PrepareSlidePictureModel(slide.PictureModel = new ContentSliderSlidePictureModel(), slide.Picture, slide.SlideTitle);
            }
            if (slide.SlideType == (int)SlideType.ProductSlide)
            {
                var product = _productService.GetProductById(slide.ItemId.Value);
                slide.ProductDetails = _helper.PrepareProductDetailsPageModel(product, new Services.Catalog.Modelling.ProductVariantQuery());
                slide.SlideTitle = slide.ProductDetails.Name;
                slide.SlideContent = slide.ProductDetails.ShortDescription;

                PrepareSlidePictureModel(slide.PictureModel = new ContentSliderSlidePictureModel(), slide.Picture, slide.ProductDetails.Name);
            }
            if (slide.SlideType == (int)SlideType.CategorySlide)
            {
                var category = _categoryService.GetCategoryById(slide.ItemId.Value);
                slide.SlideTitle = category.Name;
                slide.SlideContent = category.Description;
                slide.CategoryDetails = category.ToModel();

                PrepareSlidePictureModel(slide.PictureModel = new ContentSliderSlidePictureModel(), slide.Picture, slide.SlideTitle);
            }
            if (slide.SlideType == (int)SlideType.BrandSlide)
            {
                var manufacturer = _manufacturerService.GetManufacturerById(slide.ItemId.Value);
                slide.SlideTitle = manufacturer.Name;
                slide.SlideContent = manufacturer.Description;
                slide.ManufacturerDetails = manufacturer.ToModel();

                PrepareSlidePictureModel(slide.PictureModel = new ContentSliderSlidePictureModel(), slide.Picture, slide.SlideTitle);
            }
        }


        public void PrepareSlidePictureModel(
            ContentSliderSlidePictureModel model,
            Picture picture,
            string name)
        {
            model.Name = name;
            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            model.PictureZoomType = _mediaSettings.PictureZoomType;
            model.AlternateText = T("Media.Product.ImageAlternateTextFormat", model.Name);

            Picture defaultPicture = null;
            int defaultPictureSize = _mediaSettings.SliderPictureSize;

            using (var scope = new DbContextScope(_services.DbContext, autoCommit: false))
            {
                // Scope this part: it's quite possible that IPictureService.UpdatePicture()
                // is called when a picture is new or its size is missing in DB.

                if (picture != null)
                {
                    model.PictureModel = CreatePictureModel(model, picture, _mediaSettings.SliderPictureSize);

                    model.GalleryStartIndex = 0;
                    defaultPicture = picture;
                }

                scope.Commit();
            }

            if (defaultPicture == null)
            {
                model.DefaultPictureModel = new PictureModel
                {
                    Title = T("Media.Product.ImageLinkTitleFormat", model.Name),
                    AlternateText = model.AlternateText
                };

                model.DefaultPictureModel.Size = defaultPictureSize;
                model.DefaultPictureModel.ThumbImageUrl = _pictureService.GetFallbackUrl(_mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);
                model.DefaultPictureModel.ImageUrl = _pictureService.GetFallbackUrl(defaultPictureSize);
                model.DefaultPictureModel.FullSizeImageUrl = _pictureService.GetFallbackUrl();
            }
            else
            {
                model.DefaultPictureModel = CreatePictureModel(model, defaultPicture, defaultPictureSize);
            }
        }

        private PictureModel CreatePictureModel(ContentSliderSlidePictureModel model, Picture picture, int pictureSize)
        {
            var info = _pictureService.GetPictureInfo(picture);

            var result = new PictureModel
            {
                PictureId = info?.Id ?? 0,
                Size = pictureSize,
                ThumbImageUrl = _pictureService.GetUrl(info, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
                ImageUrl = _pictureService.GetUrl(info, pictureSize, false),
                FullSizeImageUrl = _pictureService.GetUrl(info, 0, false),
                FullSizeImageWidth = info?.Width,
                FullSizeImageHeight = info?.Height,
                Title = model.Name,
                AlternateText = model.AlternateText
            };

            return result;
        }
    }
}