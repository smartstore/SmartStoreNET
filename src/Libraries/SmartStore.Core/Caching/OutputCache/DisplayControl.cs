using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Topics;
using SmartStore.Utilities;

namespace SmartStore.Core.Caching
{
	public partial class DisplayControl : IDisplayControl
	{
		public static readonly HashSet<Type> CandidateTypes = new HashSet<Type>(new Type[] 
		{
			typeof(BlogComment),
			typeof(BlogPost),
			typeof(Category),
			typeof(Manufacturer),
			typeof(Product),
			typeof(ProductBundleItem),
			typeof(ProductPicture),
			typeof(SpecificationAttribute),
			typeof(ProductSpecificationAttribute),
			typeof(SpecificationAttributeOption),
			typeof(ProductVariantAttribute),
			typeof(ProductVariantAttributeValue),
			typeof(ProductVariantAttributeCombination),
			typeof(TierPrice),
			typeof(Discount),
			typeof(CrossSellProduct),
			typeof(RelatedProduct),
			typeof(ProductCategory),
			typeof(ProductManufacturer),
			typeof(NewsItem),
			typeof(NewsComment),
			typeof(Topic)
		});

		private readonly HashSet<BaseEntity> _entities = new HashSet<BaseEntity>();
		private readonly Lazy<IRepository<ProductSpecificationAttribute>> _rsProductSpecAttr;

		private bool _isIdle;
		private bool? _isUncacheableRequest;

		public DisplayControl(Lazy<IRepository<ProductSpecificationAttribute>> rsProductSpecAttr)
		{
			_rsProductSpecAttr = rsProductSpecAttr;
		}

		public IDisposable BeginIdleScope()
		{
			_isIdle = true;
			return new ActionDisposable(() => _isIdle = false);
		}

		public virtual void Announce(BaseEntity entity)
		{
			if (!_isIdle && entity != null)
			{
				_entities.Add(entity);
			}
		}

		public bool IsDisplayed(BaseEntity entity)
		{
			if (entity == null)
				return false;

			return _entities.Contains(entity);
		}

		public void MarkRequestAsUncacheable()
		{
			// First wins: subsequent calls should not be able to cancel this
			if (!_isIdle)
				_isUncacheableRequest = true;
		}

		public bool IsUncacheableRequest
		{
			get
			{
				return _isUncacheableRequest.GetValueOrDefault() == true;
			}
		}

		public virtual IEnumerable<string> GetCacheControlTagsFor(BaseEntity entity)
		{
			Guard.NotNull(entity, nameof(entity));

			if (entity.IsTransientRecord())
			{
				yield break;
			}

			var type = entity.GetUnproxiedType();

			if (!CandidateTypes.Contains(type))
			{
				yield break;
			}

			if (type == typeof(BlogComment))
			{
				yield return "b" + ((BlogComment)entity).BlogPostId;
			}
			else if (type == typeof(BlogPost))
			{
				yield return "b" + entity.Id;
			}
			else if (type == typeof(Category))
			{
				yield return "c" + entity.Id;
			}
			else if (type == typeof(Manufacturer))
			{
				yield return "m" + entity.Id;
			}
			else if (type == typeof(Product))
			{
				var product = ((Product)entity);
				yield return "p" + entity.Id;
				if (product.ProductType == ProductType.GroupedProduct && product.ParentGroupedProductId > 0)
				{
					yield return "p" + product.ParentGroupedProductId;
				}
			}
			else if (type == typeof(ProductTag))
			{
				var ids = ((ProductTag)entity).Products.Select(x => x.Id);
				foreach (var id in ids)
				{
					yield return "p" + id;
				}
			}
			else if (type == typeof(ProductBundleItem))
			{
				yield return "p" + ((ProductBundleItem)entity).ProductId;
			}
			else if (type == typeof(ProductPicture))
			{
				yield return "p" + ((ProductPicture)entity).ProductId;
			}
			else if (type == typeof(SpecificationAttribute))
			{
				// Determine all affected products (which are assigned to this attribute).
				var specAttrId = ((SpecificationAttribute)entity).Id;
				var affectedProductIds = _rsProductSpecAttr.Value.TableUntracked
					.Where(x => x.SpecificationAttributeOption.SpecificationAttribute.Id == specAttrId)
					.Select(x => x.ProductId)
					.Distinct()
					.ToList();

				foreach (var id in affectedProductIds)
				{
					yield return "p" + id;
				}
			}
			else if (type == typeof(ProductSpecificationAttribute))
			{
				yield return "p" + ((ProductSpecificationAttribute)entity).ProductId;
			}
			else if (type == typeof(SpecificationAttributeOption))
			{
				foreach (var attr in ((SpecificationAttributeOption)entity).ProductSpecificationAttributes)
				{
					yield return "p" + attr.ProductId;
				}
			}
			else if (type == typeof(ProductVariantAttribute))
			{
				yield return "p" + ((ProductVariantAttribute)entity).ProductId;
			}
			else if (type == typeof(ProductVariantAttributeValue))
			{
				var pva = ((ProductVariantAttributeValue)entity).ProductVariantAttribute;
				if (pva != null)
				{
					yield return "p" + pva.ProductId;
				}
			}
			else if (type == typeof(ProductVariantAttributeCombination))
			{
				yield return "p" + ((ProductVariantAttributeCombination)entity).ProductId;
			}
			else if (type == typeof(TierPrice))
			{
				yield return "p" + ((TierPrice)entity).ProductId;
			}
			else if (type == typeof(Discount))
			{
				var discount = (Discount)entity;
				if (discount.DiscountType == DiscountType.AssignedToCategories)
				{
					foreach (var category in discount.AppliedToCategories)
					{
						yield return "c" + category.Id;
					}
				}
				else if (discount.DiscountType == DiscountType.AssignedToSkus)
				{
					foreach (var product in discount.AppliedToProducts)
					{
						yield return "p" + product.Id;
					}
				}
			}
			else if (type == typeof(CrossSellProduct))
			{
				yield return "p" + ((CrossSellProduct)entity).ProductId1;
				yield return "p" + ((CrossSellProduct)entity).ProductId2;
			}
			else if (type == typeof(RelatedProduct))
			{
				yield return "p" + ((RelatedProduct)entity).ProductId1;
				yield return "p" + ((RelatedProduct)entity).ProductId2;
			}
			else if (type == typeof(ProductCategory))
			{
				yield return "c" + ((ProductCategory)entity).CategoryId;
				yield return "p" + ((ProductCategory)entity).ProductId;
			}
			else if (type == typeof(ProductManufacturer))
			{
				yield return "m" + ((ProductManufacturer)entity).ManufacturerId;
				yield return "p" + ((ProductManufacturer)entity).ProductId;
			}
			else if (type == typeof(NewsItem))
			{
				yield return "n" + entity.Id;
			}
			else if (type == typeof(NewsComment))
			{
				yield return "n" + ((NewsComment)entity).NewsItemId;
			}
			else if (type == typeof(Topic))
			{
				yield return "t" + entity.Id;
			}
		}

		public IEnumerable<string> GetAllCacheControlTags()
		{
			var tags = _entities
				.Where(x => x.Id > 0)
				.SelectMany(x => GetCacheControlTagsFor(x))
				.Where(x => x != null)
				.Distinct()
				.ToArray();

			return tags;
		}
	}
}
