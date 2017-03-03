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
	/// <summary>
	/// Deletes localized properties of entities that were deleted by referential integrity.
	/// Otherwise these localized properties would remain in the database.
	/// </summary>
	public class LocalizedEntityRelationHook : DbSaveHook<ILocalizedEntityRelation>
	{
		private readonly Lazy<IRepository<ProductAttribute>> _productAttributeRepository;
		private readonly Lazy<ILocalizedEntityService> _localizedEntityService;
		private readonly Lazy<IProductAttributeService> _productAttributeService;
		private readonly Lazy<ISpecificationAttributeService> _specificationAttributeService;

		private readonly HashSet<LocalizedProperty> _toDelete = new HashSet<LocalizedProperty>();

		public LocalizedEntityRelationHook(
			Lazy<IRepository<ProductAttribute>> productAttributeRepository,
			Lazy<ILocalizedEntityService> localizedEntityService,
			Lazy<IProductAttributeService> productAttributeService,
			Lazy<ISpecificationAttributeService> specificationAttributeService)
		{
			_productAttributeRepository = productAttributeRepository;
			_localizedEntityService = localizedEntityService;
			_productAttributeService = productAttributeService;
			_specificationAttributeService = specificationAttributeService;
		}

		protected override void OnDeleting(ILocalizedEntityRelation entity, HookedEntity entry)
		{
			var entityType = entry.Entity.GetUnproxiedType();

			if (entityType.Name.IsCaseInsensitiveEqual("SpecificationAttribute"))
			{
				var attribute = (SpecificationAttribute)entry.Entity;
				var attributeOptions = attribute.SpecificationAttributeOptions.ToList();
				if (!attributeOptions.Any())
					attributeOptions = _specificationAttributeService.Value.GetSpecificationAttributeOptionsBySpecificationAttribute(attribute.Id).ToList();

				attributeOptions.ForEach(x => _toDelete.AddRange(_localizedEntityService.Value.GetLocalizedProperties(x.Id, "SpecificationAttributeOption")));
			}
			else if (entityType.Name.IsCaseInsensitiveEqual("ProductVariantAttribute"))
			{
				var attribute = (ProductVariantAttribute)entry.Entity;
				var attributeOptions = attribute.ProductVariantAttributeValues.ToList();
				if (!attributeOptions.Any())
					attributeOptions = _productAttributeService.Value.GetProductVariantAttributeValues(attribute.Id).ToList();

				attributeOptions.ForEach(x => _toDelete.AddRange(_localizedEntityService.Value.GetLocalizedProperties(x.Id, "ProductVariantAttributeValue")));
			}
			else if (entityType.Name.IsCaseInsensitiveEqual("ProductAttribute"))
			{
				var optionIds = (
					from a in _productAttributeRepository.Value.TableUntracked
					from os in a.ProductAttributeOptionsSets
					from ao in os.ProductAttributeOptions
					where a.Id == entry.Entity.Id
					select ao.Id).ToList();

				optionIds.ForEach(x => _toDelete.AddRange(_localizedEntityService.Value.GetLocalizedProperties(x, "ProductAttributeOption")));
			}
			else if (entityType.Name.IsCaseInsensitiveEqual("ProductAttributeOptionsSet"))
			{
				var set = (ProductAttributeOptionsSet)entry.Entity;
				var attributeOptions = set.ProductAttributeOptions.ToList();
				if (!attributeOptions.Any())
					attributeOptions = _productAttributeService.Value.GetProductAttributeOptionsByOptionsSetId(set.Id).ToList();

				attributeOptions.ForEach(x => _toDelete.AddRange(_localizedEntityService.Value.GetLocalizedProperties(x.Id, "ProductAttributeOption")));
			}
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
