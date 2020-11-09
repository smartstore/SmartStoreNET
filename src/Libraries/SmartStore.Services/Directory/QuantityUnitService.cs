using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Events;
using SmartStore.Data.Caching;
using SmartStore.Services.Customers;

namespace SmartStore.Services.Directory
{
    public partial class QuantityUnitService : IQuantityUnitService
    {
        private readonly IRepository<QuantityUnit> _quantityUnitRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly CatalogSettings _catalogSettings;

        public QuantityUnitService(
            IRepository<QuantityUnit> quantityUnitRepository,
            IRepository<Product> productRepository,
            IRepository<ProductVariantAttributeCombination> attributeCombinationRepository,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            CatalogSettings catalogSettings)
        {
            this._quantityUnitRepository = quantityUnitRepository;
            this._eventPublisher = eventPublisher;
            this._productRepository = productRepository;
            this._catalogSettings = catalogSettings;
        }

        public virtual void DeleteQuantityUnit(QuantityUnit quantityUnit)
        {
            if (quantityUnit == null)
                throw new ArgumentNullException("quantityUnit");

            if (this.IsAssociated(quantityUnit.Id))
                throw new SmartException("The quantity unit cannot be deleted. It has associated product variants");

            _quantityUnitRepository.Delete(quantityUnit);
        }

        public virtual bool IsAssociated(int quantityUnitId)
        {
            if (quantityUnitId == 0)
                return false;

            var query =
                from p in _productRepository.Table
                where p.QuantityUnitId == quantityUnitId || p.ProductVariantAttributeCombinations.Any(c => c.QuantityUnitId == quantityUnitId)
                select p.Id;

            return query.Count() > 0;
        }

        public virtual QuantityUnit GetQuantityUnitById(int? quantityUnitId)
        {
            if (quantityUnitId == null || quantityUnitId == 0)
            {
                if (_catalogSettings.ShowDefaultQuantityUnit)
                {
                    return GetAllQuantityUnits().Where(x => x.IsDefault == true).FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }

            return _quantityUnitRepository.GetById(quantityUnitId);
        }

        public virtual QuantityUnit GetQuantityUnit(Product product)
        {
            if (product == null)
                return null;

            return GetQuantityUnitById(product.QuantityUnitId ?? 0);
        }

        public virtual IList<QuantityUnit> GetAllQuantityUnits()
        {
            var query = _quantityUnitRepository.Table.OrderBy(c => c.DisplayOrder);

            var quantityUnits = query.ToListCached("db.qtyunit.all");
            return quantityUnits;
        }

        public virtual void InsertQuantityUnit(QuantityUnit quantityUnit)
        {
            if (quantityUnit == null)
                throw new ArgumentNullException("quantityUnit");

            _quantityUnitRepository.Insert(quantityUnit);
        }

        public virtual void UpdateQuantityUnit(QuantityUnit quantityUnit)
        {
            if (quantityUnit == null)
                throw new ArgumentNullException("quantityUnit");

            if (quantityUnit.IsDefault == true)
            {

                var temp = new List<QuantityUnit>();
                temp.Add(quantityUnit);

                var query = GetAllQuantityUnits()
                    .Where(x => x.IsDefault == true)
                    .Except(temp);

                foreach (var qu in query)
                {
                    qu.IsDefault = false;
                    _quantityUnitRepository.Update(qu);
                }
            }

            _quantityUnitRepository.Update(quantityUnit);
        }
    }
}