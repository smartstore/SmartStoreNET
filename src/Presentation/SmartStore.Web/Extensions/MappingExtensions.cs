using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Customer;

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
        /// Creates a customer avatar model.
        /// </summary>
        /// <param name="customer">Customer entity.</param>
        /// <param name="genericAttributeService">Generic attribute service.</param>
        /// <param name="pictureService">Picture service.</param>
        /// <param name="customerSettings">Customer settings.</param>
        /// <param name="mediaSettings">Media settings.</param>
        /// <param name="urlHelper">URL helper.</param>
        /// <param name="userName">User name.</param>
        /// <param name="large">Large size.</param>
        /// <returns>Customer avatar model.</returns>
        public static CustomerAvatarModel ToAvatarModel(
            this Customer customer,
            IGenericAttributeService genericAttributeService,
            IPictureService pictureService,
            CustomerSettings customerSettings,
            MediaSettings mediaSettings,
            UrlHelper urlHelper,
            string userName = null,
            bool large = false)
        {
            Guard.NotNull(customer, nameof(customer));

            var model = new CustomerAvatarModel
            {
                Large = large,
                UserName = userName
            };

            if (customer.IsGuest())
            {
                model.AvatarLetter = 'G';
                model.AvatarColor = "light";
            }
            else
            {
                if (customer.FirstName.HasValue())
                {
                    model.AvatarLetter = customer.FirstName.First();
                }
                else if (customer.LastName.HasValue())
                {
                    model.AvatarLetter = customer.LastName.First();
                }
                else if (customer.FullName.HasValue())
                {
                    model.AvatarLetter = customer.FullName.First();
                }
                else if (customer.Username.HasValue())
                {
                    model.AvatarLetter = customer.Username.First();
                }
                else if (userName.HasValue())
                {
                    model.AvatarLetter = userName.First();
                }
                else
                {
                    model.AvatarLetter = '?';
                }

                if (customerSettings.AllowViewingProfiles)
                {
                    model.LinkUrl = urlHelper.RouteUrl("CustomerProfile", new { id = customer.Id });
                }

                if (customerSettings.AllowCustomersToUploadAvatars)
                {
                    var avatarId = customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId, genericAttributeService);
                    model.PictureUrl = pictureService.GetUrl(avatarId, mediaSettings.AvatarPictureSize, FallbackPictureType.NoFallback);
                }

                if (model.PictureUrl.IsEmpty())
                {
                    model.AvatarColor = customer.GetAvatarColor(genericAttributeService);
                }
            }

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