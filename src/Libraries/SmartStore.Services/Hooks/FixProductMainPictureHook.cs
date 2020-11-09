using System;
using System.Collections.Generic;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Data.Utilities;

namespace SmartStore.Services.Common
{
    public class FixProductMainPictureHook : DbSaveHook<ProductMediaFile>
    {
        private readonly IRepository<Product> _rsProduct;
        private readonly HashSet<Product> _products = new HashSet<Product>();

        public FixProductMainPictureHook(IRepository<Product> rsProduct)
        {
            _rsProduct = rsProduct;
        }

        protected override void OnDeleting(ProductMediaFile entity, IHookedEntity entry)
        {
            Fix(entity);
        }

        protected override void OnInserting(ProductMediaFile entity, IHookedEntity entry)
        {
            Fix(entity);
        }

        protected override void OnUpdating(ProductMediaFile entity, IHookedEntity entry)
        {
            Fix(entity);
        }

        private void Fix(ProductMediaFile entity)
        {
            var product = entity.Product ?? _rsProduct.GetById(entity.ProductId);
            if (product != null)
            {
                _products.Add(product);
            }
        }

        public override void OnBeforeSaveCompleted()
        {
            foreach (var product in _products)
            {
                DataMigrator.FixProductMainPictureId(_rsProduct.Context, product);
            }

            _products.Clear();
        }
    }
}
