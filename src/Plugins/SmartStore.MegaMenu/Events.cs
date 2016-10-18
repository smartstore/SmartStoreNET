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
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.MegaMenu
{
    public class Events :
        IConsumer<TabStripCreated>,
        IConsumer<ModelBoundEvent>,
        IConsumer<EntityUpdated<Category>>
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly IMegaMenuService _megaMenuService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public Events(IStoreContext storeContext,
            ISettingService settingService,
            IPermissionService permissionService,
            IMegaMenuService megaMenuService,
            ILocalizedEntityService localizedEntityService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _permissionService = permissionService;
            _megaMenuService = megaMenuService;
            _localizedEntityService = localizedEntityService;
        }

        public void HandleEvent(TabStripCreated eventMessage)
        {
            var tabStripName = eventMessage.TabStripName;
            var entityId = ((EntityModelBase)eventMessage.Model).Id;

            if (tabStripName == "category-edit")
            {
                eventMessage.ItemFactory.Add().Text("Mega Menu")
                    .Name("tab-MegaMenu")
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

            var model = eventMessage.BoundModel.CustomProperties["MegaMenu"] as DropdownConfigurationModel;
            if (model == null)
                return;

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return;

            var utcNow = DateTime.UtcNow;
            var record = _megaMenuService.GetMegaMenuRecord(model.CategoryId);
            var insert = (record == null);
            if (record == null)
            {
                record = new MegaMenuRecord()
                {
                    CategoryId = model.CategoryId,
                    CreatedOnUtc = utcNow
                };
            }

            record.UpdatedOnUtc = utcNow;
            record.AllowSubItemsColumnWrap = model.AllowSubItemsColumnWrap;
            record.BgAlignX = model.BgAlignX;
            record.BgAlignY = model.BgAlignY;
            record.BgLink = model.BgLink;
            record.BgOffsetX = model.BgOffsetX;
            record.BgOffsetY = model.BgOffsetY;
            record.BgPictureId = model.BgPictureId;
            record.CategoryId = model.CategoryId;
            record.DisplayBgPicture = model.DisplayBgPicture;
            record.DisplayCategoryPicture = model.DisplayCategoryPicture;
            record.DisplaySubItemsInline = model.DisplaySubItemsInline;
            record.FavorInMegamenu = model.FavorInMegamenu;
            record.HtmlColumnSpan = model.HtmlColumnSpan;
            record.IsActive = model.IsActive;
            record.MaxItemsPerColumn = model.MaxItemsPerColumn;
            record.MaxRotatorItems = model.MaxRotatorItems;
            record.MaxSubItemsPerCategory = model.MaxSubItemsPerCategory;
            record.MinChildCategoryThreshold = model.MinChildCategoryThreshold;
            record.RotatorHeading = model.RotatorHeading;
            record.SubItemsWrapTolerance = model.SubItemsWrapTolerance;
            record.Summary = model.Summary;
            record.TeaserHtml = model.TeaserHtml;
            record.TeaserRotatorItemSelectType = model.TeaserRotatorItemSelectType;
            record.TeaserRotatorProductIds = model.TeaserRotatorProductIds;
            record.TeaserType = model.TeaserType;

            //locales
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(record, x => x.BgLink, localized.BgLink, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(record, x => x.RotatorHeading, localized.RotatorHeading, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(record, x => x.Summary, localized.Summary, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(record, x => x.TeaserHtml, localized.TeaserHtml, localized.LanguageId);
            }

            if (insert)
            {
                _megaMenuService.InsertMegaMenuRecord(record);
            }
            else
            {
                _megaMenuService.UpdateMegaMenuRecord(record);
            }
        }

        public void HandleEvent(EntityUpdated<Category> eventMessage)
        {
            // delete megamenurecord when corresponding category becomes deleted
            if(eventMessage.Entity.Deleted)
            { 
                var megaMenuRecord = _megaMenuService.GetMegaMenuRecord(eventMessage.Entity.Id);
                _megaMenuService.DeleteMegaMenuRecord(megaMenuRecord);
            }
        }
    }
}