using System;
using SmartStore.Core;
using SmartStore.Core.Events;
using SmartStore.Services.Configuration;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Events;
using System.Web.Mvc.Html;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.MegaMenu.Models;
using SmartStore.MegaMenu.Settings;
using SmartStore.Services.Common;
using SmartStore.Services.Catalog;
using SmartStore.MegaMenu.Services;
using SmartStore.MegaMenu.Domain;

namespace SmartStore.MegaMenu
{
    public class Events :
        IConsumer<TabStripCreated>,
        IConsumer<ModelBoundEvent>
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly IMegaMenuService _megaMenuService;

        public Events(IStoreContext storeContext,
            ISettingService settingService,
            IPermissionService permissionService,
            IMegaMenuService megaMenuService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _permissionService = permissionService;
            _megaMenuService = megaMenuService;
        }

        public void HandleEvent(TabStripCreated eventMessage)
        {
            var tabStripName = eventMessage.TabStripName;
            var entityId = ((EntityModelBase)eventMessage.Model).Id;

            if (tabStripName == "category-edit")
            {
                eventMessage.ItemFactory.Add().Text("Mega Menu")
                    .Name("tab-MegaMenu")
                    .Icon("fa fa-bars fa-lg fa-fw")
                    .LinkHtmlAttributes(new { data_tab_name = "MegaMenu" })
                    .Route("SmartStore.MegaMenu", new { action = "AdminEditTab", categoryId = entityId })
                    .Ajax();
            }
        }
        
        [AdminAuthorize]
        public void HandleEvent(ModelBoundEvent eventMessage)
        {
            if (!eventMessage.BoundModel.CustomProperties.ContainsKey("MegaMenu"))
                return;

            var model = eventMessage.BoundModel.CustomProperties["MegaMenu"] as MegaMenuRecord;
            if (model == null)
                return;

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return;

            //var settings = _settingService.LoadSetting<MegaMenuSettings>(_storeContext.CurrentStore.Id);

            var utcNow = DateTime.UtcNow;
            var insert = (model.CreatedOnUtc == null);

            model.BadgeLabel = (BadgeLabelType)model.BadgeLabel;

            //var megaMenuRecord = _megaMenuService.GetMegaMenuRecord(model.CategoryId);

            //var insert = (megaMenuRecord == null);
            //if (megaMenuRecord == null)
            //{
            //    megaMenuRecord = new MegaMenuRecord
            //    {
            //        CategoryId = model.CategoryId,
            //        CreatedOnUtc = utcNow
            //    };
            //}

            //megaMenuRecord.CategoryId = model.CategoryId;
            //megaMenuRecord.IsActive = model.IsActive;
            //megaMenuRecord.DisplayCategoryPicture = model.DisplayCategoryPicture;
            //megaMenuRecord.DisplayBgPicture = model.DisplayBgPicture;
            //megaMenuRecord.BgPictureId = model.BgPictureId;
            //megaMenuRecord.BgLink = model.BgLink;
            ////megaMenuRecord.BgAlignX = model.BgAlignX;
            ////megaMenuRecord.BgAlignY = model.BgAlignY;
            //megaMenuRecord.BgOffsetX = model.BgOffsetX;
            //megaMenuRecord.BgOffsetY = model.BgOffsetY;
            //megaMenuRecord.MaxItemsPerColumn = model.MaxItemsPerColumn;
            //megaMenuRecord.MaxSubItemsPerCategory = model.MaxSubItemsPerCategory;
            //megaMenuRecord.Summary = model.Summary;
            //megaMenuRecord.TeaserHtml = model.TeaserHtml;
            //megaMenuRecord.HtmlColumnSpan = model.HtmlColumnSpan;
            //megaMenuRecord.TeaserType = model.TeaserType;
            //megaMenuRecord.TeaserRotatorItemSelectType = model.TeaserRotatorItemSelectType;
            //megaMenuRecord.TeaserRotatorProductIds = model.TeaserRotatorProductIds;
            //megaMenuRecord.BadgeText = model.BadgeText;
            //megaMenuRecord.BadgeLabel = model.BadgeLabel;
            //megaMenuRecord.DisplaySubItemsInline = model.DisplaySubItemsInline;
            //megaMenuRecord.AllowSubItemsColumnWrap = model.AllowSubItemsColumnWrap;
            //megaMenuRecord.SubItemsWrapTolerance = model.SubItemsWrapTolerance;
            //megaMenuRecord.FavorInMegamenu = model.FavorInMegamenu;

            // TODO: write model into record
            //megamenu.MyValue = model.MyValue;

            if (insert)
            {
                model.CreatedOnUtc = utcNow;
                model.UpdatedOnUtc = utcNow;
                _megaMenuService.InsertMegaMenuRecord(model);
            }
            else
            {
                model.UpdatedOnUtc = utcNow;
                _megaMenuService.UpdateMegaMenuRecord(model);
            }
        }
    }
}