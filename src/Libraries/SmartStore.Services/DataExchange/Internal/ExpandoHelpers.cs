using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.DataExchange.ExportTask;
using SmartStore.Services.Localization;
using SmartStore.Services.Seo;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.DataExchange.Internal
{
	internal class ExpandoHelpers
	{
		private readonly ExportProfileTaskContext _ctx;
		private readonly IUrlRecordService _urlRecordService;
		private readonly ILocalizedEntityService _localizedEntityService;

		public ExpandoHelpers(
			ExportProfileTaskContext context,
			IUrlRecordService urlRecordService,
			ILocalizedEntityService localizedEntityService)
		{
			_ctx = context;
			_urlRecordService = urlRecordService;
			_localizedEntityService = localizedEntityService;
		}

		public dynamic ToExpando(Currency currency)
		{
			if (currency == null)
				return null;

			dynamic expando = new ExpandoEntity(currency);
			expando.Name = currency.GetLocalized(x => x.Name, _ctx.Projection.LanguageId ?? 0, true, false);
			expando._Localized = GetLocalized(currency, x => x.Name);

			return expando;
		}

		private dynamic ToExpando(Language language)
		{
			if (language == null)
				return null;

			dynamic expando = new ExpandoEntity(language);
			return expando;
		}

		private dynamic ToExpando(Country country)
		{
			if (country == null)
				return null;

			dynamic expando = new ExpandoEntity(country);
			expando.Name = country.GetLocalized(x => x.Name, _ctx.Projection.LanguageId ?? 0, true, false);
			expando._Localized = GetLocalized(country, x => x.Name);

			return expando;
		}

		private dynamic ToExpando(Address address)
		{
			if (address == null)
				return null;

			dynamic expando = new ExpandoEntity(address);
			expando.Country = ToExpando(address.Country);

			if (address.StateProvinceId.GetValueOrDefault() > 0)
			{
				dynamic sp = new ExpandoEntity(address.StateProvince);
				sp.Name = address.StateProvince.GetLocalized(x => x.Name, _ctx.Projection.LanguageId ?? 0, true, false);
				sp._Localized = GetLocalized(address.StateProvince, x => x.Name);
				expando.StateProvince = sp;
			}
			else
			{
				expando.StateProvince = null;
			}

			return expando;
		}

		private dynamic ToExpando(RewardPointsHistory points)
		{
			if (points == null)
				return null;

			dynamic expando = new ExpandoEntity(points);
			return expando;
		}

		private dynamic ToExpando( Customer customer)
		{
			if (customer == null)
				return null;

			dynamic expando = new ExpandoEntity(customer);

			expando.BillingAddress = null;
			expando.ShippingAddress = null;
			expando.Addresses = null;
			expando.CustomerRoles = null;

			expando.RewardPointsHistory = null;
			expando._RewardPointsBalance = 0;

			expando._GenericAttributes = null;
			expando._HasNewsletterSubscription = false;

			return expando;
		}

		private dynamic ToExpando(Store store)
		{
			if (store == null)
				return null;

			dynamic expando = new ExpandoEntity(store);
			expando.PrimaryStoreCurrency = ToExpando(store.PrimaryStoreCurrency);
			expando.PrimaryExchangeRateCurrency = ToExpando(store.PrimaryExchangeRateCurrency);

			return expando;
		}

		private dynamic ToExpando(DeliveryTime deliveryTime)
		{
			if (deliveryTime == null)
				return null;

			dynamic expando = new ExpandoEntity(deliveryTime);
			expando.Name = deliveryTime.GetLocalized(x => x.Name, _ctx.Projection.LanguageId ?? 0, true, false);
			expando._Localized = GetLocalized(deliveryTime, x => x.Name);

			return expando;
		}

		private dynamic ToExpando(QuantityUnit quantityUnit)
		{
			if (quantityUnit == null)
				return null;

			dynamic expando = new ExpandoEntity(quantityUnit);
			expando.Name = quantityUnit.GetLocalized(x => x.Name, _ctx.Projection.LanguageId ?? 0, true, false);
			expando.Description = quantityUnit.GetLocalized(x => x.Description, _ctx.Projection.LanguageId ?? 0, true, false);
			expando._Localized = GetLocalized(quantityUnit,
				x => x.Name,
				x => x.Description);

			return expando;
		}

		private dynamic ToExpando(Picture picture, int thumbPictureSize, int detailsPictureSize)
		{
			if (picture == null)
				return null;

			dynamic expando = new ExpandoEntity(picture);

			//// TODO!!!!!
			//expando._ThumbImageUrl = _pictureService.Value.GetPictureUrl(picture, thumbPictureSize, false, _ctx.Store.Url);
			//expando._ImageUrl = _pictureService.Value.GetPictureUrl(picture, detailsPictureSize, false, _ctx.Store.Url);
			//expando._FullSizeImageUrl = _pictureService.Value.GetPictureUrl(picture, 0, false, _ctx.Store.Url);

			//var relativeUrl = _pictureService.Value.GetPictureUrl(picture);
			//expando._FileName = relativeUrl.Substring(relativeUrl.LastIndexOf("/") + 1);

			//expando._ThumbLocalPath = _pictureService.Value.GetThumbLocalPath(picture);

			return expando;
		}

		// TODO: weitermachen [...]

		private List<dynamic> GetLocalized<T>(T entity, params Expression<Func<T, string>>[] keySelectors)
			where T : BaseEntity, ILocalizedEntity
		{
			if (_ctx.Languages.Count <= 1)
				return null;

			var localized = new List<dynamic>();

			var localeKeyGroup = typeof(T).Name;
			var isSlugSupported = typeof(ISlugSupported).IsAssignableFrom(typeof(T));

			foreach (var language in _ctx.Languages)
			{
				var languageCulture = language.Value.LanguageCulture.EmptyNull().ToLower();

				// add SeName
				if (isSlugSupported)
				{
					var value = _urlRecordService.GetActiveSlug(entity.Id, localeKeyGroup, language.Value.Id);
					if (value.HasValue())
					{
						dynamic exp = new ExpandoObject();
						exp.Culture = languageCulture;
						exp.LocaleKey = "SeName";
						exp.LocaleValue = value;

						localized.Add(exp);
					}
				}

				foreach (var keySelector in keySelectors)
				{
					var member = keySelector.Body as MemberExpression;
					var propInfo = member.Member as PropertyInfo;
					string localeKey = propInfo.Name;
					var value = _localizedEntityService.GetLocalizedValue(language.Value.Id, entity.Id, localeKeyGroup, localeKey);

					// we better not export empty values. The risk is too high that they get reimported and unnecessarily pollute databases.
					if (value.HasValue())
					{
						dynamic exp = new ExpandoObject();
						exp.Culture = languageCulture;
						exp.LocaleKey = localeKey;
						exp.LocaleValue = value;

						localized.Add(exp);
					}
				}
			}

			return (localized.Count == 0 ? null : localized);
		}
	}
}
