using SmartStore.MegaMenu.Models;
using SmartStore.MegaMenu.Services;
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
		private readonly IMegaMenuService _megaMenuService;
        private readonly CatalogHelper _helper;

        public MegaMenuFilter(CatalogHelper helper /*IMegaMenuService megaMenuService*/)
		{
            //_megaMenuService = megaMenuService;
            _helper = helper;
        }
        
		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
		}

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //var model = _helper.PrepareCategoryNavigationModel(1, 0);
            //filterContext.Controller.ViewData.Model = model;
            //filterContext.Result = new PartialViewResult { ViewName = "~/Plugins/SmartStore.MegaMenu/Views/MegaMenu/MegaMenu.cshtml", ViewData = new ViewDataDictionary(model) };

            var model = GetModel(filterContext.Controller.ViewData.Model as NavigationModel);

            filterContext.Result = new PartialViewResult
            {
                ViewName = "~/Plugins/SmartStore.MegaMenu/Views/MegaMenu/MegaMenu.cshtml",
                ViewData = new ViewDataDictionary(filterContext.Controller.ViewData.Model)
            };
        }

        public MegaMenuNavigationModel GetModel(NavigationModel navigationModel)
        {
            var model = new MegaMenuNavigationModel();

            model.NavigationModel = navigationModel;

            // TODO 
            //model.Settings = GetSettings();
            //model.DropdownModels = new Dictionary<int, MegaMenuDropdownModel>();

            //foreach(var cat in navigationModel.Root.Children)
            //{
            //    var megaMenuRecord = _megaMenuService.GetMegaMenuRecord(cat.Value.EntityId);
            //    model.DropdownModels.Add(cat.Value.EntityId, new MegaMenuDropdownModel
            //    {
            //        AllowSubItemsColumnWrap = megaMenuRecord.AllowSubItemsColumnWrap,
            //        BgCss = megaMenuRecord.BgAlignX + Domain.AlignX, // TODO
            //        BgLink = megaMenuRecord.BgLink,
            //        BgPicturePath = megaMenuRecord.BgPictureId,
            //        DisplayBgPicture = megaMenuRecord.DisplayBgPicture,
            //        DisplayCategoryPicture = megaMenuRecord.DisplayCategoryPicture,
            //        DisplaySubItemsInline = megaMenuRecord.DisplaySubItemsInline,
            //        HtmlColumnSpan = megaMenuRecord.HtmlColumnSpan,
            //        IsActive = megaMenuRecord.IsActive,
            //        MaxItemsPerColumn = megaMenuRecord.MaxItemsPerColumn,
            //        MaxSubItemsPerCategory = megaMenuRecord.MaxSubItemsPerCategory,
            //        SubItemsWrapTolerance = megaMenuRecord.SubItemsWrapTolerance,
            //        Summary = megaMenuRecord.Summary,
            //        TeaserHtml = megaMenuRecord.TeaserHtml,
            //        TeaserRotatorItemSelectType = megaMenuRecord.TeaserRotatorItemSelectType,
            //        TeaserRotatorProductIds = megaMenuRecord.TeaserRotatorProductIds,
            //        TeaserType = megaMenuRecord.TeaserType
            //    });
            //}

            return model; 
        }
    }
}