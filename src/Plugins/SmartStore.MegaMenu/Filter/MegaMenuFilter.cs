using SmartStore.MegaMenu.Domain;
using SmartStore.MegaMenu.Models;
using SmartStore.MegaMenu.Services;
using SmartStore.MegaMenu.Settings;
using SmartStore.Services;
using SmartStore.Services.Media;
using SmartStore.Web.Controllers;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Services.Localization;

namespace SmartStore.MegaMenu.Filters
{
	public class MegaMenuFilter : IActionFilter
	{
        private readonly ICommonServices _services;
        private readonly IMegaMenuService _megaMenuService;
        private readonly IPictureService _pictureService;
        private readonly CatalogHelper _helper;
        private readonly MegaMenuSettings _megaMenuSettings;
        
        public MegaMenuFilter(CatalogHelper helper, 
            IMegaMenuService megaMenuService,
            ICommonServices services,
            IPictureService pictureService,
            MegaMenuSettings megaMenuSettings)
		{
            _services = services;
            _megaMenuService = megaMenuService;
            _pictureService = pictureService;
            _helper = helper;
            _megaMenuSettings = megaMenuSettings;
        }
        
		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
		}

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var model = GetModel(filterContext.Controller.ViewData.Model as NavigationModel);

            filterContext.Result = new PartialViewResult
            {
                ViewName = "~/Plugins/SmartStore.MegaMenu/Views/MegaMenu/MegaMenu.cshtml",
                ViewData = new ViewDataDictionary(model)
            };
        }

        public MegaMenuNavigationModel GetModel(NavigationModel navigationModel)
        {
            var model = new MegaMenuNavigationModel();
            
            var cacheKey = _megaMenuService.GetCacheKey(
                _services.StoreContext.CurrentStore.Id, 
                _services.WorkContext.CurrentCustomer.CustomerRoles.Select(x => x.Id).ToString(),
                _services.WorkContext.WorkingLanguage.Id);

            var cachedResult = _services.Cache.Get(cacheKey, () => 
            {
                var dropdownModels = new Dictionary<int, MegaMenuDropdownModel>();

                var catIds = navigationModel.Root.Children.Select(x => x.Value.EntityId).ToArray();
                var recordsMap = _megaMenuService.GetMegaMenuRecords(catIds).ToDictionarySafe(x => x.CategoryId);

                foreach (var catId in catIds)
                {
                    var record = recordsMap.Get(catId) ?? new MegaMenuRecord { CategoryId = catId };
                    var dropdownModel = new MegaMenuDropdownModel
                    {
                        AllowSubItemsColumnWrap = record.AllowSubItemsColumnWrap,
                        BgCss = GetContainerAlignmentCss(record.BgAlignX, record.BgAlignY, record.BgOffsetX.ToString(), record.BgOffsetY.ToString()),
                        BgLink = record.GetLocalized(x => x.BgLink),
                        BgPicturePath = _pictureService.GetPictureUrl(record.BgPictureId),
                        DisplayBgPicture = record.DisplayBgPicture,
                        DisplayCategoryPicture = record.DisplayCategoryPicture,
                        DisplaySubItemsInline = record.DisplaySubItemsInline,
                        HtmlColumnSpan = record.HtmlColumnSpan,
                        IsActive = record.IsActive,
                        MaxItemsPerColumn = record.MaxItemsPerColumn,
                        MaxSubItemsPerCategory = record.MaxSubItemsPerCategory,
                        MinChildCategoryThreshold = record.MinChildCategoryThreshold,
                        SubItemsWrapTolerance = record.SubItemsWrapTolerance,
                        Summary = record.GetLocalized(x => x.Summary),
                        TeaserHtml = record.GetLocalized(x => x.TeaserHtml),
                        TeaserRotatorItemSelectType = record.TeaserRotatorItemSelectType,
                        TeaserRotatorProductIds = record.TeaserRotatorProductIds,
                        TeaserType = record.TeaserType,
                        RotatorHeading = record.GetLocalized(x => x.RotatorHeading)
                    };

                    dropdownModels[catId] = dropdownModel;
                }

                return dropdownModels;
            });

            foreach (var child in navigationModel.Root.Children)
            {
                child.SetThreadMetadata("MegamenuModel", cachedResult.Get(child.Value.EntityId));
            }

            model.NavigationModel = navigationModel;
            model.Settings = _megaMenuSettings;

            return model;
        }

        private string GetContainerAlignmentCss(AlignX alignmentX, AlignY alignmentY, string OffsetX, string OffsetY)
        {
            // alignmentX = top || bottom
            // alignmentY = left || center || right

            var css = String.Empty;

            if (alignmentX.Equals("top"))
            {
                css += "top:0;";
            }
            else
            {
                css += "bottom:0;";
            }

            if (alignmentY.Equals("left"))
            {
                css += "left:0;";
            }
            else if (alignmentY.Equals("center"))
            {
                css += "width: 100%;text-align: center;";
            }
            else
            {
                css += "bottom:0;";
            }

            css += "transform: translate({0}px, {1}px);".FormatWith(OffsetX, OffsetY);

            return css;
        }
    }
}