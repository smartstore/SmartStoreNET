using SmartStore.MegaMenu.Domain;
using SmartStore.MegaMenu.Models;
using SmartStore.MegaMenu.Services;
using SmartStore.MegaMenu.Settings;
using SmartStore.Services;
using SmartStore.Services.Media;
using SmartStore.Web.Controllers;
using SmartStore.Web.Framework.UI;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.MegaMenu.Filters
{
	public class MegaMenuFilter : IActionFilter
	{
        private readonly ICommonServices _services;
        private readonly IMegaMenuService _megaMenuService;
        private readonly IPictureService _pictureService;
        private readonly CatalogHelper _helper;
        
        public MegaMenuFilter(CatalogHelper helper, 
            IMegaMenuService megaMenuService,
            ICommonServices services,
            IPictureService pictureService)
		{
            _services = services;
            _megaMenuService = megaMenuService;
            _pictureService = pictureService;
            _helper = helper;
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

            model.NavigationModel = navigationModel;
            
            foreach (var cat in navigationModel.Root.Children)
            {
                var megaMenuRecord = _megaMenuService.GetMegaMenuRecord(cat.Value.EntityId);

                var dropdownModel = new MegaMenuDropdownModel
                {
                    AllowSubItemsColumnWrap = megaMenuRecord.AllowSubItemsColumnWrap,
                    BgCss = GetContainerAlignmentCss(megaMenuRecord.BgAlignX, megaMenuRecord.BgAlignY, megaMenuRecord.BgOffsetX.ToString(), megaMenuRecord.BgOffsetY.ToString()),
                    BgLink = megaMenuRecord.BgLink,
                    BgPicturePath = _pictureService.GetPictureUrl(megaMenuRecord.BgPictureId),
                    DisplayBgPicture = megaMenuRecord.DisplayBgPicture,
                    DisplayCategoryPicture = megaMenuRecord.DisplayCategoryPicture,
                    DisplaySubItemsInline = megaMenuRecord.DisplaySubItemsInline,
                    HtmlColumnSpan = megaMenuRecord.HtmlColumnSpan,
                    IsActive = megaMenuRecord.IsActive,
                    MaxItemsPerColumn = megaMenuRecord.MaxItemsPerColumn,
                    MaxSubItemsPerCategory = megaMenuRecord.MaxSubItemsPerCategory,
                    SubItemsWrapTolerance = megaMenuRecord.SubItemsWrapTolerance,
                    Summary = megaMenuRecord.Summary,
                    TeaserHtml = megaMenuRecord.TeaserHtml,
                    TeaserRotatorItemSelectType = megaMenuRecord.TeaserRotatorItemSelectType,
                    TeaserRotatorProductIds = megaMenuRecord.TeaserRotatorProductIds,
                    TeaserType = megaMenuRecord.TeaserType

                    //CloseColumn ???
                };

                cat.SetThreadMetadata("MegamenuModel", dropdownModel);
            }

            // get plugin settings
            model.Settings = _services.Settings.LoadSetting<MegaMenuSettings>(_services.StoreContext.CurrentStore.Id);

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