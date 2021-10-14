using QTRADO.WMAddOn.Models;
using QTRADO.WMAddOn.Settings;

using SmartStore.Core;
using SmartStore.Core.Events;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;

namespace QTRADO.WMAddOn
{
    public class ModelBoundEventConsumer : IConsumer
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IProductService _productService;

        public ModelBoundEventConsumer(IStoreContext storeContext,
            ISettingService settingService,
            IGenericAttributeService genericAttributeService,
            IProductService productService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _genericAttributeService = genericAttributeService;
            _productService = productService;
        }

        // This method will be executed when the model of the containing entity is bound.
        // You can use it to store the data the shop admin has entered in the injected tab.
        [AdminAuthorize]
        public void HandleEvent(ModelBoundEvent eventMessage)
        {
            if (!eventMessage.BoundModel.CustomProperties.ContainsKey("WMAddOn"))
                return;

            var model = eventMessage.BoundModel.CustomProperties["WMAddOn"] as AdminEditTabModel;
            if (model == null)
                return;

            var settings = _settingService.LoadSetting<WMAddOnSettings>(_storeContext.CurrentStore.Id);

            var currentProduct = _productService.GetProductById(model.EntityId);




            //#####################################################################################
            // To save model values in own plugin repositories use service methods as shown below.
            //#####################################################################################

            //var entity = _myService.GetMyRecord(model.EntityId, model.EntityName);
            //var insert = (entity == null);
            //if (entity == null)
            //{
            //    entity = new MyRecord
            //    {
            //        EntityId = model.EntityId,
            //        EntityName = model.EntityName,
            //    };
            //}

            //entity.MyValue = model.MyValue;

            //if (insert)
            //{
            //    _myService.InsertMyRecord(entity);
            //}
            //else
            //{
            //    _myService.UpdateMyRecord(entity);
            //}

        }
    }
}