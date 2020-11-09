using System;
using System.Collections.Generic;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.Hooks
{
    public class ProductVariantAttributeValueHook : DbSaveHook<ProductVariantAttributeValue>
    {
        private readonly Lazy<IProductAttributeService> _productAttributeService;
        private readonly HashSet<ProductVariantAttributeValue> _toDelete = new HashSet<ProductVariantAttributeValue>();

        public ProductVariantAttributeValueHook(Lazy<IProductAttributeService> productAttributeService)
        {
            _productAttributeService = productAttributeService;
        }

        protected override void OnDeleted(ProductVariantAttributeValue entity, IHookedEntity entry)
        {
            _toDelete.Add(entity);
        }

        public override void OnAfterSaveCompleted()
        {
            if (_toDelete.Count == 0)
                return;

            using (var scope = new DbContextScope(autoCommit: false))
            {
                _toDelete.Each(x => _productAttributeService.Value.DeleteProductBundleItemAttributeFilter(x.ProductVariantAttributeId, x.Id));
                scope.Commit();
            }

            _toDelete.Clear();
        }
    }
}
