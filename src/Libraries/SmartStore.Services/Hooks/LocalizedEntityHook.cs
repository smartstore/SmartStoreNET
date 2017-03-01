using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Hooks
{
	public class LocalizedEntityHook : DbSaveHook<ILocalizedEntity>
	{
		private readonly Lazy<ILocalizedEntityService> _localizedEntityService;
		private readonly Lazy<IProductAttributeService> _productAttributeService;
		private readonly Lazy<ISpecificationAttributeService> _specificationAttributeService;
		private readonly HashSet<LocalizedProperty> _toDelete = new HashSet<LocalizedProperty>();

		public LocalizedEntityHook(
			Lazy<ILocalizedEntityService> localizedEntityService,
			Lazy<IProductAttributeService> productAttributeService,
			Lazy<ISpecificationAttributeService> specificationAttributeService)
		{
			_localizedEntityService = localizedEntityService;
			_productAttributeService = productAttributeService;
			_specificationAttributeService = specificationAttributeService;
		}

		protected override void OnDeleting(ILocalizedEntity entity, HookedEntity entry)
		{
			var entityType = entry.Entity.GetUnproxiedType();

			// Delete localized properties of entities that were deleted by referential integrity.
			if (entityType.Name.IsCaseInsensitiveEqual("ProductVariantAttribute"))
			{
				var attribute = (ProductVariantAttribute)entry.Entity;
				var attributeOptions = attribute.ProductVariantAttributeValues.ToList();
				if (!attributeOptions.Any())
					attributeOptions = _productAttributeService.Value.GetProductVariantAttributeValues(attribute.Id).ToList();

				attributeOptions.ForEach(x => _toDelete.AddRange(_localizedEntityService.Value.GetLocalizedProperties(x.Id, "ProductVariantAttributeValue")));
			}
			else if (entityType.Name.IsCaseInsensitiveEqual("ProductAttribute"))
			{
				var attribute = (ProductAttribute)entry.Entity;
				var attributeOptions = attribute.ProductAttributeOptions.ToList();
				if (!attributeOptions.Any())
					attributeOptions = _productAttributeService.Value.GetProductAttributeOptionByAttributeId(attribute.Id).ToList();

				attributeOptions.ForEach(x => _toDelete.AddRange(_localizedEntityService.Value.GetLocalizedProperties(x.Id, "ProductAttributeOption")));
			}
			else if (entityType.Name.IsCaseInsensitiveEqual("SpecificationAttribute"))
			{
				var attribute = (SpecificationAttribute)entry.Entity;
				var attributeOptions = attribute.SpecificationAttributeOptions.ToList();
				if (!attributeOptions.Any())
					attributeOptions = _specificationAttributeService.Value.GetSpecificationAttributeOptionsBySpecificationAttribute(attribute.Id).ToList();

				attributeOptions.ForEach(x => _toDelete.AddRange(_localizedEntityService.Value.GetLocalizedProperties(x.Id, "SpecificationAttributeOption")));
			}
		}

		protected override void OnDeleted(ILocalizedEntity entity, HookedEntity entry)
		{
			var entityType = entry.Entity.GetUnproxiedType();
			var localizedEntities = _localizedEntityService.Value.GetLocalizedProperties(entry.Entity.Id, entityType.Name);
			_toDelete.AddRange(localizedEntities);
		}

		public override void OnAfterSaveCompleted()
		{
			if (_toDelete.Count == 0)
				return;

			using (var scope = new DbContextScope(autoCommit: false))
			{
				using (_localizedEntityService.Value.BeginScope())
				{
					_toDelete.Each(x => _localizedEntityService.Value.DeleteLocalizedProperty(x));
				}

				scope.Commit();
				_toDelete.Clear();
			}
		}
	}

}
