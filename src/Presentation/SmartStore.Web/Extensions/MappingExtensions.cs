using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Seo;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web
{
    public static class MappingExtensions
    {
        // Category
        public static CategoryModel ToModel(this Category entity)
        {
			// TODO: (mc) delete later
			if (entity == null)
                return null;

            var model = new CategoryModel
            {
                Id = entity.Id,
                Name = entity.GetLocalized(x => x.Name),
				FullName = entity.GetLocalized(x => x.FullName),
                Description = entity.GetLocalized(x => x.Description, detectEmptyHtml: true),
				BottomDescription = entity.GetLocalized(x => x.BottomDescription, detectEmptyHtml: true),
                MetaKeywords = entity.GetLocalized(x => x.MetaKeywords),
                MetaDescription = entity.GetLocalized(x => x.MetaDescription),
                MetaTitle = entity.GetLocalized(x => x.MetaTitle),
                SeName = entity.GetSeName(),
            };
            return model;
        }

		// Manufacturer
		public static ManufacturerModel ToModel(this Manufacturer entity)
        {
            if (entity == null)
                return null;

            var model = new ManufacturerModel
            {
                Id = entity.Id,
                Name = entity.GetLocalized(x => x.Name),
                Description = entity.GetLocalized(x => x.Description, detectEmptyHtml: true),
                MetaKeywords = entity.GetLocalized(x => x.MetaKeywords),
                MetaDescription = entity.GetLocalized(x => x.MetaDescription),
                MetaTitle = entity.GetLocalized(x => x.MetaTitle),
                SeName = entity.GetSeName(),
            };
            return model;
        }

        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="address">Address</param>
        /// <param name="excludeProperties">A value indicating whether to exclude properties</param>
        /// <param name="addressSettings">Address settings</param>
        /// <param name="localizationService">Localization service (used to prepare a select list)</param>
        /// <param name="stateProvinceService">State service (used to prepare a select list). null to don't prepare the list.</param>
        /// <param name="loadCountries">A function to load countries  (used to prepare a select list). null to don't prepare the list.</param>
        public static void PrepareModel(this AddressModel model,
            Address address, 
			bool excludeProperties,
            AddressSettings addressSettings,
            ILocalizationService localizationService = null,
            IStateProvinceService stateProvinceService = null,
            Func<IList<Country>> loadCountries = null)
        {
			Guard.NotNull(model, nameof(model));
			Guard.NotNull(addressSettings, nameof(addressSettings));

			// Form fields
			MiniMapper.Map(addressSettings, model);

			if (!excludeProperties && address != null)
            {
				MiniMapper.Map(address, model);

				model.EmailMatch = address.Email;
				model.CountryName = address.Country?.GetLocalized(x => x.Name);
				if (address.StateProvinceId.HasValue && address.StateProvince != null)
				{
					model.StateProvinceName = address.StateProvince.GetLocalized(x => x.Name);
				}
				model.FormattedAddress = Core.Infrastructure.EngineContext.Current.Resolve<IAddressService>().FormatAddress(address, true);
			}

            // Countries and states
            if (addressSettings.CountryEnabled && loadCountries != null)
            {
                if (localizationService == null)
                    throw new ArgumentNullException("localizationService");

                model.AvailableCountries.Add(new SelectListItem { Text = localizationService.GetResource("Address.SelectCountry"), Value = "0" });
                foreach (var c in loadCountries())
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.GetLocalized(x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (addressSettings.StateProvinceEnabled)
                {
                    // States
                    if (stateProvinceService == null)
                        throw new ArgumentNullException("stateProvinceService");

                    var states = stateProvinceService
                        .GetStateProvincesByCountryId(model.CountryId ?? 0)
                        .ToList();
                    if (states.Count > 0)
                    {
                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem
                            {
                                Text = s.GetLocalized(x => x.Name),
                                Value = s.Id.ToString(),
                                Selected = (s.Id == model.StateProvinceId)
                            });
                        }
                    }
                    else
                    {
                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = localizationService.GetResource("Address.OtherNonUS"),
                            Value = "0"
                        });
                    }
                }
            }
            
            if (localizationService != null)
            {
                string salutations = addressSettings.GetLocalized(x => x.Salutations);
                foreach (var sal in salutations.SplitSafe(","))
                {
                    model.AvailableSalutations.Add(new SelectListItem { Value = sal, Text = sal });
                }
            }
        }

        public static Address ToEntity(this AddressModel model)
        {
            if (model == null)
                return null;

            var entity = new Address();
            return ToEntity(model, entity);
        }

        public static Address ToEntity(this AddressModel model, Address destination)
        {
            if (model == null)
                return destination;

            destination.Id = model.Id;
            destination.Salutation = model.Salutation;
            destination.Title = model.Title;
            destination.FirstName = model.FirstName;
            destination.LastName = model.LastName;
            destination.Email = model.Email;
            destination.Company = model.Company;
            destination.CountryId = model.CountryId;
            destination.StateProvinceId = model.StateProvinceId;
            destination.City = model.City;
            destination.Address1 = model.Address1;
            destination.Address2 = model.Address2;
            destination.ZipPostalCode = model.ZipPostalCode;
            destination.PhoneNumber = model.PhoneNumber;
            destination.FaxNumber = model.FaxNumber;

            return destination;
        }
    }
}