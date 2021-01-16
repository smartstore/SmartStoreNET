using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Security;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Common;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.DataExchange.Import.Events;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Media;
using SmartStore.Utilities;

namespace SmartStore.Services.Customers.Importer
{
    public class CustomerImporter : EntityImporterBase
    {
        private const string _attributeKeyGroup = "Customer";

        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly ICommonServices _services;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IMediaService _mediaService;
        private readonly IAffiliateService _affiliateService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly FileDownloadManager _fileDownloadManager;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ForumSettings _forumSettings;
        private readonly TaxSettings _taxSettings;
        private readonly PrivacySettings _privacySettings;

        public CustomerImporter(
            IRepository<Customer> customerRepository,
            IRepository<CustomerRole> customerRoleRepository,
            ICommonServices services,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IMediaService mediaService,
            IAffiliateService affiliateService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            FileDownloadManager fileDownloadManager,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            ForumSettings forumSettings,
            TaxSettings taxSettings,
            PrivacySettings privacySettings)
        {
            _customerRepository = customerRepository;
            _customerRoleRepository = customerRoleRepository;
            _mediaService = mediaService;
            _services = services;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _affiliateService = affiliateService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _fileDownloadManager = fileDownloadManager;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _forumSettings = forumSettings;
            _taxSettings = taxSettings;
            _privacySettings = privacySettings;
        }

        protected override void Import(ImportExecuteContext context)
        {
            var customer = _services.WorkContext.CurrentCustomer;
            var allowManagingCustomerRoles = _services.Permissions.Authorize(Permissions.Customer.EditRole, customer);

            var allAffiliateIds = _affiliateService.GetAllAffiliates(true)
                .Select(x => x.Id)
                .ToList();

            var allCountries = new Dictionary<string, int>();
            foreach (var country in _countryService.GetAllCountries(true))
            {
                if (!allCountries.ContainsKey(country.TwoLetterIsoCode))
                    allCountries.Add(country.TwoLetterIsoCode, country.Id);

                if (!allCountries.ContainsKey(country.ThreeLetterIsoCode))
                    allCountries.Add(country.ThreeLetterIsoCode, country.Id);
            }

            var allStateProvinces = _stateProvinceService.GetAllStateProvinces(true)
                .ToDictionarySafe(x => new Tuple<int, string>(x.CountryId, x.Abbreviation), x => x.Id);

            var allCustomerNumbers = new HashSet<string>(
                _customerRepository.Table.Where(x => !string.IsNullOrEmpty(x.CustomerNumber)).Select(x => x.CustomerNumber),
                StringComparer.OrdinalIgnoreCase);

            using (var scope = new DbContextScope(ctx: _services.DbContext, hooksEnabled: false, autoDetectChanges: false, proxyCreation: false, validateOnSave: false, autoCommit: false))
            {
                var segmenter = context.DataSegmenter;

                Initialize(context);
                AddInfoForDeprecatedFields(context);

                while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
                {
                    var batch = segmenter.GetCurrentBatch<Customer>();

                    _customerRepository.Context.DetachAll(true);

                    context.SetProgress(segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

                    // ===========================================================================
                    // Process customers
                    // ===========================================================================
                    try
                    {
                        ProcessCustomers(context, batch, allAffiliateIds, allCustomerNumbers);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessCustomers");
                    }

                    // reduce batch to saved (valid) records.
                    // No need to perform import operations on errored records.
                    batch = batch.Where(x => x.Entity != null && !x.IsTransient).ToArray();

                    // update result object
                    context.Result.NewRecords += batch.Count(x => x.IsNew && !x.IsTransient);
                    context.Result.ModifiedRecords += batch.Count(x => !x.IsNew && !x.IsTransient);

                    // ===========================================================================
                    // Process customer roles
                    // ===========================================================================
                    if (allowManagingCustomerRoles && context.DataSegmenter.HasColumn("CustomerRoleSystemNames"))
                    {
                        try
                        {
                            _customerRepository.AutoCommitEnabled = true;
                            ProcessCustomerRoles(context, batch);
                        }
                        catch (Exception ex)
                        {
                            context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessCustomerRoles");
                        }
                        finally
                        {
                            _customerRepository.AutoCommitEnabled = false;
                        }
                    }

                    // ===========================================================================
                    // Process generic attributes
                    // ===========================================================================
                    try
                    {
                        ProcessGenericAttributes(context, batch, allCountries, allStateProvinces);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessGenericAttributes");
                    }

                    // ===========================================================================
                    // Process avatars
                    // ===========================================================================
                    if (_customerSettings.AllowCustomersToUploadAvatars)
                    {
                        try
                        {
                            ProcessAvatars(context, batch);
                        }
                        catch (Exception ex)
                        {
                            context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessAvatars");
                        }
                    }

                    // ===========================================================================
                    // Process addresses
                    // ===========================================================================
                    try
                    {
                        _services.DbContext.AutoDetectChangesEnabled = true;
                        ProcessAddresses(context, batch, allCountries, allStateProvinces);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessAddresses");
                    }
                    finally
                    {
                        _services.DbContext.AutoDetectChangesEnabled = false;
                    }

                    context.Services.EventPublisher.Publish(new ImportBatchExecutedEvent<Customer>(context, batch));
                }
            }
        }

        protected virtual int ProcessCustomers(
            ImportExecuteContext context,
            IEnumerable<ImportRow<Customer>> batch,
            List<int> allAffiliateIds,
            HashSet<string> allCustomerNumbers)
        {
            _customerRepository.AutoCommitEnabled = true;

            var currentCustomer = _services.WorkContext.CurrentCustomer;
            var customerQuery = _customerRepository.Table
                .Expand(x => x.Addresses)
                .Expand(x => x.CustomerRoleMappings.Select(rm => rm.CustomerRole));

            foreach (var row in batch)
            {
                Customer customer = null;
                var id = row.GetDataValue<int>("Id");
                var email = row.GetDataValue<string>("Email");
                var userName = row.GetDataValue<string>("Username");

                foreach (var keyName in context.KeyFieldNames)
                {
                    switch (keyName)
                    {
                        case "Id":
                            if (id != 0)
                            {
                                customer = customerQuery.FirstOrDefault(x => x.Id == id);
                            }
                            break;
                        case "CustomerGuid":
                            var customerGuid = row.GetDataValue<string>("CustomerGuid");
                            if (customerGuid.HasValue())
                            {
                                var guid = new Guid(customerGuid);
                                customer = customerQuery.FirstOrDefault(x => x.CustomerGuid == guid);
                            }
                            break;
                        case "Email":
                            if (email.HasValue())
                            {
                                customer = customerQuery.FirstOrDefault(x => x.Email == email);
                            }
                            break;
                        case "Username":
                            if (userName.HasValue())
                            {
                                customer = customerQuery.FirstOrDefault(x => x.Username == userName);
                            }
                            break;
                    }

                    if (customer != null)
                        break;
                }

                if (customer == null)
                {
                    if (context.UpdateOnly)
                    {
                        ++context.Result.SkippedRecords;
                        continue;
                    }

                    customer = new Customer
                    {
                        CustomerGuid = new Guid(),
                        AffiliateId = 0,
                        Active = true
                    };
                }
                else
                {
                    _customerRepository.Context.LoadCollection(customer, (Customer x) => x.CustomerRoleMappings);
                }

                var affiliateId = row.GetDataValue<int>("AffiliateId");

                row.Initialize(customer, email ?? id.ToString());

                row.SetProperty(context.Result, (x) => x.CustomerGuid);
                row.SetProperty(context.Result, (x) => x.Username);
                row.SetProperty(context.Result, (x) => x.Email);
                row.SetProperty(context.Result, (x) => x.Salutation);
                row.SetProperty(context.Result, (x) => x.FullName);
                row.SetProperty(context.Result, (x) => x.FirstName);
                row.SetProperty(context.Result, (x) => x.LastName);

                if (_customerSettings.TitleEnabled)
                    row.SetProperty(context.Result, (x) => x.Title);

                if (_customerSettings.CompanyEnabled)
                    row.SetProperty(context.Result, (x) => x.Company);

                if (_customerSettings.DateOfBirthEnabled)
                    row.SetProperty(context.Result, (x) => x.BirthDate);

                if (_privacySettings.StoreLastIpAddress)
                    row.SetProperty(context.Result, (x) => x.LastIpAddress);

                if (email.HasValue() && currentCustomer.Email.IsCaseInsensitiveEqual(email))
                {
                    context.Result.AddInfo("Security. Ignored password of current customer (who started this import).", row.GetRowInfo(), "Password");
                }
                else
                {
                    row.SetProperty(context.Result, (x) => x.Password);
                    row.SetProperty(context.Result, (x) => x.PasswordFormatId);
                    row.SetProperty(context.Result, (x) => x.PasswordSalt);
                }

                row.SetProperty(context.Result, (x) => x.AdminComment);
                row.SetProperty(context.Result, (x) => x.IsTaxExempt);
                row.SetProperty(context.Result, (x) => x.Active);

                row.SetProperty(context.Result, (x) => x.CreatedOnUtc, UtcNow);
                row.SetProperty(context.Result, (x) => x.LastActivityDateUtc, UtcNow);

                if (_taxSettings.EuVatEnabled)
                    row.SetProperty(context.Result, (x) => x.VatNumberStatusId);

                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    row.SetProperty(context.Result, (x) => x.TimeZoneId);

                if (_customerSettings.GenderEnabled)
                    row.SetProperty(context.Result, (x) => x.Gender);

                if (affiliateId > 0 && allAffiliateIds.Contains(affiliateId))
                    customer.AffiliateId = affiliateId;

                string customerNumber = null;

                if (_customerSettings.CustomerNumberMethod == CustomerNumberMethod.AutomaticallySet && row.IsTransient)
                {
                    customerNumber = row.Entity.Id.ToString();
                }
                else if (_customerSettings.CustomerNumberMethod == CustomerNumberMethod.Enabled && !row.IsTransient && row.HasDataValue("CustomerNumber"))
                {
                    customerNumber = row.GetDataValue<string>("CustomerNumber");
                }

                if (customerNumber.HasValue() || !allCustomerNumbers.Contains(customerNumber))
                {
                    row.Entity.CustomerNumber = customerNumber;

                    if (!customerNumber.IsEmpty())
                    {
                        allCustomerNumbers.Add(customerNumber);
                    }
                }

                if (row.IsTransient)
                {
                    _customerRepository.Insert(customer);
                }
                else
                {
                    _customerRepository.Update(customer);
                }
            }

            var num = _customerRepository.Context.SaveChanges();
            return num;
        }

        protected virtual int ProcessCustomerRoles(
            ImportExecuteContext context,
            IEnumerable<ImportRow<Customer>> batch)
        {
            Dictionary<string, CustomerRole> allCustomerRoles = null;

            foreach (var row in batch)
            {
                var customer = row.Entity;
                var importRoleSystemNames = row.GetDataValue<List<string>>("CustomerRoleSystemNames");

                var assignedRoles = customer.CustomerRoleMappings
                    .Where(x => !x.IsSystemMapping)
                    .Select(x => x.CustomerRole)
                    .ToDictionarySafe(x => x.SystemName, StringComparer.OrdinalIgnoreCase);

                // Roles to remove.
                foreach (var customerRole in assignedRoles)
                {
                    var systemName = customerRole.Key;
                    if (!systemName.IsCaseInsensitiveEqual(SystemCustomerRoleNames.Administrators) &&
                        !systemName.IsCaseInsensitiveEqual(SystemCustomerRoleNames.SuperAdministrators) &&
                        !importRoleSystemNames.Contains(systemName))
                    {
                        var mappings = customer.CustomerRoleMappings.Where(x => !x.IsSystemMapping && x.CustomerRoleId == customerRole.Value.Id).ToList();
                        mappings.Each(x => _customerService.DeleteCustomerRoleMapping(x));
                    }
                }

                // Roles to add.
                foreach (var systemName in importRoleSystemNames)
                {
                    if (systemName.IsCaseInsensitiveEqual(SystemCustomerRoleNames.Administrators) ||
                        systemName.IsCaseInsensitiveEqual(SystemCustomerRoleNames.SuperAdministrators))
                    {
                        context.Result.AddInfo("Security. Ignored administrator role.", row.GetRowInfo(), "CustomerRoleSystemNames");
                    }
                    else if (!assignedRoles.ContainsKey(systemName))
                    {
                        // Add role mapping, never insert roles.
                        // Be careful not to insert the roles several times!
                        if (allCustomerRoles == null)
                        {
                            allCustomerRoles = _customerRoleRepository.TableUntracked
                                .Where(x => !string.IsNullOrEmpty(x.SystemName))
                                .ToDictionarySafe(x => x.SystemName, StringComparer.OrdinalIgnoreCase);
                        }

                        if (allCustomerRoles.TryGetValue(systemName, out var role))
                        {
                            _customerService.InsertCustomerRoleMapping(new CustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = role.Id });
                        }
                    }
                }
            }

            return _services.DbContext.SaveChanges();
        }

        protected virtual int ProcessAddresses(
            ImportExecuteContext context,
            IEnumerable<ImportRow<Customer>> batch,
            Dictionary<string, int> allCountries,
            Dictionary<Tuple<int, string>, int> allStateProvinces)
        {
            foreach (var row in batch)
            {
                ImportAddress("BillingAddress.", row, context, allCountries, allStateProvinces);
                ImportAddress("ShippingAddress.", row, context, allCountries, allStateProvinces);
            }

            return _services.DbContext.SaveChanges();
        }

        private void ImportAddress(
            string fieldPrefix,
            ImportRow<Customer> row,
            ImportExecuteContext context,
            Dictionary<string, int> allCountries,
            Dictionary<Tuple<int, string>, int> allStateProvinces)
        {
            // Last name is mandatory for an address to be imported or updated.
            if (!row.HasDataValue(fieldPrefix + "LastName"))
            {
                return;
            }

            Address address = null;

            if (fieldPrefix == "BillingAddress.")
            {
                address = row.Entity.BillingAddress ?? new Address { CreatedOnUtc = UtcNow };
            }
            else if (fieldPrefix == "ShippingAddress.")
            {
                address = row.Entity.ShippingAddress ?? new Address { CreatedOnUtc = UtcNow };
            }

            var childRow = new ImportRow<Address>(row.Segmenter, row.DataRow, row.Position);
            childRow.Initialize(address, row.EntityDisplayName);

            childRow.SetProperty(context.Result, fieldPrefix + "Salutation", x => x.Salutation);
            childRow.SetProperty(context.Result, fieldPrefix + "Title", x => x.Title);
            childRow.SetProperty(context.Result, fieldPrefix + "FirstName", x => x.FirstName);
            childRow.SetProperty(context.Result, fieldPrefix + "LastName", x => x.LastName);
            childRow.SetProperty(context.Result, fieldPrefix + "Email", x => x.Email);
            childRow.SetProperty(context.Result, fieldPrefix + "Company", x => x.Company);
            childRow.SetProperty(context.Result, fieldPrefix + "City", x => x.City);
            childRow.SetProperty(context.Result, fieldPrefix + "Address1", x => x.Address1);
            childRow.SetProperty(context.Result, fieldPrefix + "Address2", x => x.Address2);
            childRow.SetProperty(context.Result, fieldPrefix + "ZipPostalCode", x => x.ZipPostalCode);
            childRow.SetProperty(context.Result, fieldPrefix + "PhoneNumber", x => x.PhoneNumber);
            childRow.SetProperty(context.Result, fieldPrefix + "FaxNumber", x => x.FaxNumber);

            childRow.SetProperty(context.Result, fieldPrefix + "CountryId", x => x.CountryId);
            if (childRow.Entity.CountryId == null)
            {
                // Try with country code.
                childRow.SetProperty(context.Result, fieldPrefix + "CountryCode", x => x.CountryId, converter: (val, ci) => CountryCodeToId(allCountries, val.ToString()));
            }

            var countryId = childRow.Entity.CountryId;
            if (countryId.HasValue)
            {
                if (row.HasDataValue(fieldPrefix + "StateProvinceId"))
                {
                    childRow.SetProperty(context.Result, fieldPrefix + "StateProvinceId", x => x.StateProvinceId);
                }
                else
                {
                    // Try with state abbreviation.
                    childRow.SetProperty(context.Result, fieldPrefix + "StateAbbreviation", x => x.StateProvinceId, converter: (val, ci) => StateAbbreviationToId(allStateProvinces, countryId, val.ToString()));
                }
            }

            if (!childRow.IsDirty)
            {
                // Not one single property could be set. Get out!
                return;
            }

            if (address.Id == 0)
            {
                // Avoid importing two addresses if billing and shipping address are equal.
                var appliedAddress = row.Entity.Addresses.FindAddress(address);
                if (appliedAddress == null)
                {
                    appliedAddress = address;
                    row.Entity.Addresses.Add(appliedAddress);
                }

                // Map address to customer.
                if (fieldPrefix == "BillingAddress.")
                {
                    row.Entity.BillingAddress = appliedAddress;
                }
                else if (fieldPrefix == "ShippingAddress.")
                {
                    row.Entity.ShippingAddress = appliedAddress;
                }
            }

            _customerRepository.Update(row.Entity);
        }

        protected virtual int ProcessGenericAttributes(
            ImportExecuteContext context,
            IEnumerable<ImportRow<Customer>> batch,
            Dictionary<string, int> allCountries,
            Dictionary<Tuple<int, string>, int> allStateProvinces)
        {
            foreach (var row in batch)
            {
                if (_taxSettings.EuVatEnabled)
                {
                    SaveAttribute(row, SystemCustomerAttributeNames.VatNumber);
                }

                if (_customerSettings.StreetAddressEnabled)
                    SaveAttribute(row, SystemCustomerAttributeNames.StreetAddress);

                if (_customerSettings.StreetAddress2Enabled)
                    SaveAttribute(row, SystemCustomerAttributeNames.StreetAddress2);

                if (_customerSettings.CityEnabled)
                    SaveAttribute(row, SystemCustomerAttributeNames.City);

                if (_customerSettings.ZipPostalCodeEnabled)
                    SaveAttribute(row, SystemCustomerAttributeNames.ZipPostalCode);

                if (_customerSettings.CountryEnabled)
                    SaveAttribute<int>(row, SystemCustomerAttributeNames.CountryId);

                if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                    SaveAttribute<int>(row, SystemCustomerAttributeNames.StateProvinceId);

                if (_customerSettings.PhoneEnabled)
                    SaveAttribute(row, SystemCustomerAttributeNames.Phone);

                if (_customerSettings.FaxEnabled)
                    SaveAttribute(row, SystemCustomerAttributeNames.Fax);

                if (_forumSettings.ForumsEnabled)
                    SaveAttribute<int>(row, SystemCustomerAttributeNames.ForumPostCount);

                if (_forumSettings.SignaturesEnabled)
                    SaveAttribute(row, SystemCustomerAttributeNames.Signature);

                var countryId = CountryCodeToId(allCountries, row.GetDataValue<string>("CountryCode"));
                var stateId = StateAbbreviationToId(allStateProvinces, countryId, row.GetDataValue<string>("StateAbbreviation"));

                if (countryId.HasValue)
                    SaveAttribute(row, SystemCustomerAttributeNames.CountryId, countryId.Value);

                if (stateId.HasValue)
                    SaveAttribute(row, SystemCustomerAttributeNames.StateProvinceId, stateId.Value);

            }

            return _services.DbContext.SaveChanges();
        }

        protected virtual int ProcessAvatars(
            ImportExecuteContext context,
            IEnumerable<ImportRow<Customer>> batch)
        {
            foreach (var row in batch)
            {
                var urlOrPath = row.GetDataValue<string>("AvatarPictureUrl");
                if (urlOrPath.IsEmpty())
                {
                    continue;
                }

                var image = CreateDownloadImage(context, urlOrPath, 1);
                if (image == null)
                {
                    continue;
                }

                if (image.Url.HasValue() && !image.Success.HasValue)
                {
                    AsyncRunner.RunSync(() => _fileDownloadManager.DownloadAsync(DownloaderContext, new FileDownloadManagerItem[] { image }));
                }

                if ((image.Success ?? false) && File.Exists(image.Path))
                {
                    Succeeded(image);
                    using (var stream = File.OpenRead(image.Path))
                    {
                        if (stream?.Length > 0)
                        {
                            MediaFile sourceFile = null;
                            var currentFiles = new List<MediaFileInfo>();
                            var fileId = row.Entity.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId);
                            var file = _mediaService.GetFileById(fileId, MediaLoadFlags.AsNoTracking);

                            if (file != null)
                            {
                                currentFiles.Add(file);
                            }

                            if (_mediaService.FindEqualFile(stream, currentFiles.Select(x => x.File), true, out var _))
                            {
                                context.Result.AddInfo($"Found equal image in customer data for {image.FileName}. Skipping file.", row.GetRowInfo(), "AvatarPictureUrl");
                            }
                            else
                            {
                                // An avatar may not be assigned to several customers. A customer could otherwise delete the avatar of another.
                                // Overwriting is probably too dangerous here, because we could overwrite the avatar of another customer, so better rename.
                                var path = _mediaService.CombinePaths(SystemAlbumProvider.Customers, image.FileName);
                                sourceFile = _mediaService.SaveFile(path, stream, false, DuplicateFileHandling.Rename)?.File;
                            }

                            if (sourceFile?.Id > 0)
                            {
                                SaveAttribute(row, SystemCustomerAttributeNames.AvatarPictureId, sourceFile.Id);
                            }
                        }
                    }
                }
                else
                {
                    context.Result.AddInfo("Download of avatar failed.", row.GetRowInfo(), "AvatarPictureUrl");
                }
            }

            return _services.DbContext.SaveChanges();
        }

        public static string[] SupportedKeyFields => new string[] { "Id", "CustomerGuid", "Email", "Username" };

        public static string[] DefaultKeyFields => new string[] { "Email", "CustomerGuid" };

        private int? CountryCodeToId(Dictionary<string, int> allCountries, string code)
        {
            int countryId;
            if (code.HasValue() && allCountries.TryGetValue(code, out countryId) && countryId != 0)
            {
                return countryId;
            }

            return null;
        }

        private int? StateAbbreviationToId(Dictionary<Tuple<int, string>, int> allStateProvinces, int? countryId, string abbreviation)
        {
            if (countryId.HasValue && abbreviation.HasValue())
            {
                var key = Tuple.Create<int, string>(countryId.Value, abbreviation);

                int stateId;
                if (allStateProvinces.TryGetValue(key, out stateId) && stateId != 0)
                {
                    return stateId;
                }
            }

            return null;
        }

        private void SaveAttribute(ImportRow<Customer> row, string key)
        {
            SaveAttribute(row, key, row.GetDataValue<string>(key));
        }

        private void SaveAttribute<TPropType>(ImportRow<Customer> row, string key)
        {
            SaveAttribute(row, key, row.GetDataValue<TPropType>(key));
        }

        private void SaveAttribute<TPropType>(ImportRow<Customer> row, string key, TPropType value)
        {
            if (row.IsTransient)
                return;

            if (row.IsNew || value != null)
            {
                _genericAttributeService.SaveAttribute(row.Entity.Id, key, _attributeKeyGroup, value);
            }
        }

        private void AddInfoForDeprecatedFields(ImportExecuteContext context)
        {
            if (context.DataSegmenter.HasColumn("IsGuest"))
            {
                context.Result.AddInfo("Deprecated field. Use CustomerRoleSystemNames instead.", null, "IsGuest");
            }
            if (context.DataSegmenter.HasColumn("IsRegistered"))
            {
                context.Result.AddInfo("Deprecated field. Use CustomerRoleSystemNames instead.", null, "IsRegistered");
            }
            if (context.DataSegmenter.HasColumn("IsAdministrator"))
            {
                context.Result.AddInfo("Deprecated field. Use CustomerRoleSystemNames instead.", null, "IsAdministrator");
            }
            if (context.DataSegmenter.HasColumn("IsForumModerator"))
            {
                context.Result.AddInfo("Deprecated field. Use CustomerRoleSystemNames instead.", null, "IsForumModerator");
            }
        }
    }
}
