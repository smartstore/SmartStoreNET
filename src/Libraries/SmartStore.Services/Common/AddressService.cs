using System;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Directory;
using SmartStore.Core.Events;
using System.Collections.Generic;
using SmartStore.Templating;
using SmartStore.Services.Messages;
using SmartStore.Core.Domain.Directory;
using System.Globalization;
using SmartStore.Core.Html;

namespace SmartStore.Services.Common
{
    public partial class AddressService : IAddressService
    {
        private readonly IRepository<Address> _addressRepository;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICommonServices _services;
        private readonly AddressSettings _addressSettings;
        private readonly ITemplateEngine _templateEngine;
        private readonly IMessageModelProvider _messageModelProvider;

        public AddressService(
            IRepository<Address> addressRepository,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ICommonServices services,
            AddressSettings addressSettings,
            ITemplateEngine templateEngine,
            IMessageModelProvider messageModelProvider)
        {
            _addressRepository = addressRepository;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _services = services;
            _addressSettings = addressSettings;
            _templateEngine = templateEngine;
            _messageModelProvider = messageModelProvider;
        }

        public virtual void DeleteAddress(Address address)
        {
            Guard.NotNull(address, nameof(address));

            _addressRepository.Delete(address);
        }

        public virtual void DeleteAddress(int id)
        {
            var address = GetAddressById(id);
            if (address != null)
                DeleteAddress(address);
        }

        public virtual int GetAddressTotalByCountryId(int countryId)
        {
            if (countryId == 0)
                return 0;

            var query = from a in _addressRepository.Table
                        where a.CountryId == countryId
                        select a;
            return query.Count();
        }

        public virtual int GetAddressTotalByStateProvinceId(int stateProvinceId)
        {
            if (stateProvinceId == 0)
                return 0;

            var query = from a in _addressRepository.Table
                        where a.StateProvinceId == stateProvinceId
                        select a;
            return query.Count();
        }

        public virtual Address GetAddressById(int addressId)
        {
            if (addressId == 0)
                return null;

            var address = _addressRepository.GetById(addressId);
            return address;
        }

        public virtual IList<Address> GetAddressByIds(int[] addressIds)
        {
            Guard.NotNull(addressIds, nameof(addressIds));

            var query =
                from x in _addressRepository.TableUntracked.Expand(x => x.Country).Expand(x => x.StateProvince)
                where addressIds.Contains(x.Id)
                select x;

            return query.ToList();
        }

        public virtual void InsertAddress(Address address)
        {
            Guard.NotNull(address, nameof(address));

            address.CreatedOnUtc = DateTime.UtcNow;

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            _addressRepository.Insert(address);
        }

        public virtual void UpdateAddress(Address address)
        {
            Guard.NotNull(address, nameof(address));

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            _addressRepository.Update(address);
        }

        public virtual bool IsAddressValid(Address address)
        {
            Guard.NotNull(address, nameof(address));

            if (String.IsNullOrWhiteSpace(address.FirstName))
                return false;

            if (String.IsNullOrWhiteSpace(address.LastName))
                return false;

            if (String.IsNullOrWhiteSpace(address.Email))
                return false;

            if (_addressSettings.CompanyEnabled &&
                _addressSettings.CompanyRequired &&
                String.IsNullOrWhiteSpace(address.Company))
                return false;

            if (_addressSettings.StreetAddressEnabled &&
                _addressSettings.StreetAddressRequired &&
                String.IsNullOrWhiteSpace(address.Address1))
                return false;

            if (_addressSettings.StreetAddress2Enabled &&
                _addressSettings.StreetAddress2Required &&
                String.IsNullOrWhiteSpace(address.Address2))
                return false;

            if (_addressSettings.ZipPostalCodeEnabled &&
                _addressSettings.ZipPostalCodeRequired &&
                String.IsNullOrWhiteSpace(address.ZipPostalCode))
                return false;


            if (_addressSettings.CountryEnabled)
            {
                if (address.CountryId == null || address.CountryId.Value == 0)
                    return false;

                var country = _countryService.GetCountryById(address.CountryId.Value);
                if (country == null)
                    return false;

                if (_addressSettings.StateProvinceEnabled)
                {
                    var states = _stateProvinceService.GetStateProvincesByCountryId(country.Id);
                    if (states.Count > 0)
                    {
                        if (address.StateProvinceId == null || address.StateProvinceId.Value == 0)
                            return false;

                        var state = _stateProvinceService.GetStateProvinceById(address.StateProvinceId.Value);
                        if (state == null)
                            return false;
                    }
                }
            }

            if (_addressSettings.CityEnabled &&
                _addressSettings.CityRequired &&
                String.IsNullOrWhiteSpace(address.City))
                return false;

            if (_addressSettings.PhoneEnabled &&
                _addressSettings.PhoneRequired &&
                String.IsNullOrWhiteSpace(address.PhoneNumber))
                return false;

            if (_addressSettings.FaxEnabled &&
                _addressSettings.FaxRequired &&
                String.IsNullOrWhiteSpace(address.FaxNumber))
                return false;

            return true;
        }

        public virtual string FormatAddress(CompanyInformationSettings settings, bool newLineToBr = false)
        {
            Guard.NotNull(settings, nameof(settings));

            var address = new Address
            {
                Address1 = settings.Street,
                Address2 = settings.Street2,
                City = settings.City,
                Company = settings.CompanyName,
                FirstName = settings.Firstname,
                LastName = settings.Lastname,
                Salutation = settings.Salutation,
                Title = settings.Title,
                ZipPostalCode = settings.ZipCode,
                CountryId = settings.CountryId,
                Country = _countryService.GetCountryById(settings.CountryId)
            };

            return FormatAddress(address, newLineToBr);
        }

        public virtual string FormatAddress(Address address, bool newLineToBr = false)
        {
            Guard.NotNull(address, nameof(address));

            var messageContext = new MessageContext
            {
                Language = _services.WorkContext.WorkingLanguage,
                Store = _services.StoreContext.CurrentStore,
                Model = new TemplateModel()
            };

            _messageModelProvider.AddModelPart(address, messageContext, "Address");
            var model = messageContext.Model["Address"];

            var result = FormatAddress(model, address?.Country?.AddressFormat, messageContext.FormatProvider);

            if (newLineToBr)
            {
                result = HtmlUtils.ConvertPlainTextToHtml(result);
            }

            return result;
        }

        public virtual string FormatAddress(object address, string template = null, IFormatProvider formatProvider = null)
        {
            Guard.NotNull(address, nameof(address));

            template = template.NullEmpty() ?? Address.DefaultAddressFormat;

            var result = _templateEngine
                .Render(template, address, formatProvider ?? CultureInfo.CurrentCulture)
                .Compact(true);

            return result;
        }
    }
}