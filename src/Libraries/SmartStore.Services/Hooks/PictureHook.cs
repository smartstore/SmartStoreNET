using System;
using System.Linq;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Media;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.Hooks
{
	/// <summary>
	/// Deletes associated pictures of deleted entities
	/// </summary>
	public class PictureHook : DbSaveHook<BaseEntity>
	{
		private readonly Lazy<IPictureService> _pictureService;
		private readonly Lazy<IProductAttributeService> _productAttributeService;

		private readonly HashSet<int> _toDelete = new HashSet<int>();

		private static readonly HashSet<Type> _candidateTypes = new HashSet<Type>(new Type[]
		{
			typeof(ProductAttributeOption),
			typeof(ProductAttributeOptionsSet),
			typeof(ProductAttribute),
			typeof(ProductVariantAttribute),
			typeof(ProductVariantAttributeValue)
		});

		public PictureHook(
			Lazy<IPictureService> pictureService,
			Lazy<IProductAttributeService> productAttributeService)
		{
			_pictureService = pictureService;
			_productAttributeService = productAttributeService;
		}

		protected override void OnDeleting(BaseEntity entity, IHookedEntity entry)
		{
			var type = entry.EntityType;

			if (!_candidateTypes.Contains(type))
				throw new NotSupportedException();

			if (type == typeof(ProductAttributeOption))
			{
				var pictureId = ((ProductAttributeOption)entry.Entity).PictureId;
				if (pictureId != 0)
				{
					_toDelete.Add(pictureId);
				}
			}
			else if (type == typeof(ProductAttributeOptionsSet))
			{
				var options = _productAttributeService.Value.GetProductAttributeOptionsByOptionsSetId(entity.Id);
				_toDelete.AddRange(options.Where(x => x.PictureId != 0).Select(x => x.PictureId));
			}
			else if (type == typeof(ProductAttribute))
			{
				var options = _productAttributeService.Value.GetProductAttributeOptionsByAttributeId(entity.Id);
				_toDelete.AddRange(options.Where(x => x.PictureId != 0).Select(x => x.PictureId));
			}
			else if (type == typeof(ProductVariantAttribute))
			{
				var options = _productAttributeService.Value.GetProductVariantAttributeValues(entity.Id);
				_toDelete.AddRange(options.Where(x => x.PictureId != 0).Select(x => x.PictureId));
			}
			else if (type == typeof(ProductVariantAttributeValue))
			{
				var pictureId = ((ProductVariantAttributeValue)entry.Entity).PictureId;
				if (pictureId != 0)
				{
					_toDelete.Add(pictureId);
				}
			}
		}

		public override void OnAfterSaveCompleted()
		{
			if (_toDelete.Count == 0)
				return;

			using (var scope = new DbContextScope(autoCommit: false))
			{
				var pictures = _pictureService.Value.GetPicturesByIds(_toDelete.ToArray());

				pictures.Each(x => _pictureService.Value.DeletePicture(x));

				scope.Commit();
				_toDelete.Clear();
			}
		}
	}
}
