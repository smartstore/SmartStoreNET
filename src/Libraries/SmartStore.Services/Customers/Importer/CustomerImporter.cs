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
using SmartStore.Core.Events;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Common;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Media;
using SmartStore.Services.Security;
using SmartStore.Utilities;

namespace SmartStore.Services.Customers.Importer
{
	public class CustomerImporter : EntityImporterBase
	{
		private const string _attributeKeyGroup = "Customer";

		private readonly IRepository<Customer> _customerRepository;
		private readonly IRepository<Picture> _pictureRepository;
		private readonly ICommonServices _services;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly ICustomerService _customerService;
		private readonly IPictureService _pictureService;
		private readonly IAffiliateService _affiliateService;
		private readonly ICountryService _countryService;
		private readonly IStateProvinceService _stateProvinceService;
		private readonly FileDownloadManager _fileDownloadManager;
		private readonly CustomerSettings _customerSettings;
		private readonly DateTimeSettings _dateTimeSettings;
		private readonly ForumSettings _forumSettings;
		private readonly DataExchangeSettings _dataExchangeSettings;

		public CustomerImporter(
			IRepository<Customer> customerRepository,
			IRepository<Picture> pictureRepository,
			ICommonServices services,
			IGenericAttributeService genericAttributeService,
			ICustomerService customerService,
			IPictureService pictureService,
			IAffiliateService affiliateService,
			ICountryService countryService,
			IStateProvinceService stateProvinceService,
			FileDownloadManager fileDownloadManager,
			CustomerSettings customerSettings,
			DateTimeSettings dateTimeSettings,
			ForumSettings forumSettings,
			DataExchangeSettings dataExchangeSettings)
		{
			_customerRepository = customerRepository;
			_pictureRepository = pictureRepository;
			_services = services;
			_genericAttributeService = genericAttributeService;
			_customerService = customerService;
			_pictureService = pictureService;
			_affiliateService = affiliateService;
			_countryService = countryService;
			_stateProvinceService = stateProvinceService;
			_fileDownloadManager = fileDownloadManager;
			_customerSettings = customerSettings;
			_dateTimeSettings = dateTimeSettings;
			_forumSettings = forumSettings;
			_dataExchangeSettings = dataExchangeSettings;
		}

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

			if (row.IsNew || value!= null)
			{
				_genericAttributeService.SaveAttribute(row.Entity.Id, key, _attributeKeyGroup, value);
			}
		}

		private void UpsertRole(ImportRow<Customer> row, CustomerRole role, bool value)
		{
			if (role == null)
				return;

			var hasRole = row.Entity.CustomerRoles.Any(x => x.SystemName == role.SystemName);

			if (value && !hasRole)
			{
				row.Entity.CustomerRoles.Add(role);
			}
			else if (!value && hasRole)
			{
				row.Entity.CustomerRoles.Remove(role);
			}
		}

		protected virtual int ProcessAddresses(
			IImportExecuteContext context,
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
			IImportExecuteContext context,
			Dictionary<string, int> allCountries,
			Dictionary<Tuple<int, string>, int> allStateProvinces)
		{
			// last name is mandatory for an address to be imported
			var lastName = row.GetDataValue<string>(fieldPrefix + "LastName");
			if (lastName.IsEmpty())
				return;

			var countryId = CountryCodeToId(allCountries, row.GetDataValue<string>(fieldPrefix + "CountryCode"));
			var stateId = StateAbbreviationToId(allStateProvinces, countryId, row.GetDataValue<string>(fieldPrefix + "StateAbbreviation"));

			var importAddress = new Address
			{
				LastName = lastName,
				CreatedOnUtc = UtcNow,
				CountryId = countryId,
				StateProvinceId = stateId
			};

			var childRow = new ImportRow<Address>(row.Segmenter, row.DataRow, row.Position);
			childRow.Initialize(importAddress, row.EntityDisplayName);

			childRow.SetProperty(context.Result, fieldPrefix + "FirstName", x => x.FirstName);
			childRow.SetProperty(context.Result, fieldPrefix + "Email", x => x.Email);
			childRow.SetProperty(context.Result, fieldPrefix + "Company", x => x.Company);
			childRow.SetProperty(context.Result, fieldPrefix + "City", x => x.City);
			childRow.SetProperty(context.Result, fieldPrefix + "Address1", x => x.Address1);
			childRow.SetProperty(context.Result, fieldPrefix + "Address2", x => x.Address2);
			childRow.SetProperty(context.Result, fieldPrefix + "ZipPostalCode", x => x.ZipPostalCode);
			childRow.SetProperty(context.Result, fieldPrefix + "PhoneNumber", x => x.PhoneNumber);
			childRow.SetProperty(context.Result, fieldPrefix + "FaxNumber", x => x.FaxNumber);

			var appliedAddress = row.Entity.Addresses.FindAddress(importAddress);

			if (appliedAddress == null)
			{
				appliedAddress = importAddress;
				row.Entity.Addresses.Add(appliedAddress);
			}

			if (fieldPrefix == "BillingAddress.")
			{
				row.Entity.BillingAddress = appliedAddress;
			}
			else if (fieldPrefix == "ShippingAddress.")
			{
				row.Entity.ShippingAddress = appliedAddress;
			}

			_customerService.UpdateCustomer(row.Entity);
		}

		protected virtual int ProcessGenericAttributes(
			IImportExecuteContext context,
			IEnumerable<ImportRow<Customer>> batch,
			Dictionary<string, int> allCountries,
			Dictionary<Tuple<int, string>, int> allStateProvinces,
			HashSet<string> allCustomerNumbers)
		{
			foreach (var row in batch)
			{
				SaveAttribute(row, SystemCustomerAttributeNames.FirstName);
				SaveAttribute(row, SystemCustomerAttributeNames.LastName);

				if (_dateTimeSettings.AllowCustomersToSetTimeZone)
					SaveAttribute(row, SystemCustomerAttributeNames.TimeZoneId);

				if (_customerSettings.GenderEnabled)
					SaveAttribute(row, SystemCustomerAttributeNames.Gender);

				if (_customerSettings.DateOfBirthEnabled)
					SaveAttribute<DateTime?>(row, SystemCustomerAttributeNames.DateOfBirth);

				if (_customerSettings.CompanyEnabled)
					SaveAttribute(row, SystemCustomerAttributeNames.Company);

				if (_customerSettings.StreetAddressEnabled)
					SaveAttribute(row, SystemCustomerAttributeNames.StreetAddress);

				if (_customerSettings.StreetAddress2Enabled)
					SaveAttribute(row, SystemCustomerAttributeNames.StreetAddress2);

				if (_customerSettings.ZipPostalCodeEnabled)
					SaveAttribute(row, SystemCustomerAttributeNames.ZipPostalCode);

				if (_customerSettings.CityEnabled)
					SaveAttribute(row, SystemCustomerAttributeNames.City);

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
				{
					SaveAttribute(row, SystemCustomerAttributeNames.CountryId, countryId.Value);
				}

				if (stateId.HasValue)
				{
					SaveAttribute(row, SystemCustomerAttributeNames.StateProvinceId, stateId.Value);
				}

				string customerNumber = null;

				if (_customerSettings.CustomerNumberMethod == CustomerNumberMethod.AutomaticallySet)
					customerNumber = row.Entity.Id.ToString();
				else
					customerNumber = row.GetDataValue<string>("CustomerNumber");

				if (customerNumber.IsEmpty() || !allCustomerNumbers.Contains(customerNumber))
				{
					SaveAttribute(row, SystemCustomerAttributeNames.CustomerNumber, customerNumber);

					if (!customerNumber.IsEmpty())
						allCustomerNumbers.Add(customerNumber);
				}
			}

			return _services.DbContext.SaveChanges();
		}

		protected virtual int ProcessAvatars(
			IImportExecuteContext context,
			IEnumerable<ImportRow<Customer>> batch)
		{
			foreach (var row in batch)
			{
				var urlOrPath = row.GetDataValue<string>("AvatarPictureUrl");
				if (urlOrPath.IsEmpty())
					continue;

				Picture picture = null;
				var equalPictureId = 0;
				var currentPictures = new List<Picture>();
				var seoName = _pictureService.GetPictureSeName(row.EntityDisplayName);

				var image = CreateDownloadImage(urlOrPath, seoName, 1);
				if (image == null)
					continue;

				if (image.Url.HasValue() && !image.Success.HasValue)
				{
					AsyncRunner.RunSync(() => _fileDownloadManager.DownloadAsync(DownloaderContext, new FileDownloadManagerItem[] { image }));
				}

				if ((image.Success ?? false) && File.Exists(image.Path))
				{
					Succeeded(image);
					var pictureBinary = File.ReadAllBytes(image.Path);

					if (pictureBinary != null && pictureBinary.Length > 0)
					{
						var currentPictureId = row.Entity.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId);
						if (currentPictureId != 0 && (picture = _pictureRepository.GetById(currentPictureId)) != null)
						{
							currentPictures.Add(picture);
						}

						pictureBinary = _pictureService.ValidatePicture(pictureBinary);
						pictureBinary = _pictureService.FindEqualPicture(pictureBinary, currentPictures, out equalPictureId);

						if (pictureBinary != null && pictureBinary.Length > 0)
						{
							if ((picture = _pictureService.InsertPicture(pictureBinary, image.MimeType, seoName, true, false, false)) != null)
							{
								_pictureRepository.Context.SaveChanges();
								SaveAttribute(row, SystemCustomerAttributeNames.AvatarPictureId, picture.Id);
							}
						}
						else
						{
							context.Result.AddInfo("Found equal picture in data store. Skipping field.", row.GetRowInfo(), "AvatarPictureUrl");
						}
					}
				}
				else
				{
					context.Result.AddInfo("Download of an image failed.", row.GetRowInfo(), "AvatarPictureUrl");
				}
			}

			return _services.DbContext.SaveChanges();
		}

		protected virtual int ProcessCustomers(
			IImportExecuteContext context,
			IEnumerable<ImportRow<Customer>> batch,
			List<int> allAffiliateIds)
		{
			_customerRepository.AutoCommitEnabled = true;

			Customer lastInserted = null;
			Customer lastUpdated = null;
			var currentCustomer = _services.WorkContext.CurrentCustomer;

			var guestRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests);
			var registeredRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
			var forumModeratorRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.ForumModerators);

			foreach (var row in batch)
			{
				Customer customer = null;
				var id = row.GetDataValue<int>("Id");
				var email = row.GetDataValue<string>("Email");

				foreach (var keyName in context.KeyFieldNames)
				{
					switch (keyName)
					{
						case "Id":
							customer = _customerService.GetCustomerById(id);
							break;
						case "CustomerGuid":
							var guid = row.GetDataValue<string>("CustomerGuid");
							if (guid.HasValue())
								customer = _customerService.GetCustomerByGuid(new Guid(guid));
							break;
						case "Email":
							customer = _customerService.GetCustomerByEmail(email);
							break;
						case "Username":
							customer = _customerService.GetCustomerByUsername(row.GetDataValue<string>("Username"));
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
					_customerRepository.Context.LoadCollection(customer, (Customer x) => x.CustomerRoles);
				}

				var isGuest = row.GetDataValue<bool>("IsGuest");
				var isRegistered = row.GetDataValue<bool>("IsRegistered");
				var isAdmin = row.GetDataValue<bool>("IsAdministrator");
				var isForumModerator = row.GetDataValue<bool>("IsForumModerator");
				var affiliateId = row.GetDataValue<int>("AffiliateId");

				row.Initialize(customer, email ?? id.ToString());

				row.SetProperty(context.Result, (x) => x.CustomerGuid);
				row.SetProperty(context.Result, (x) => x.Username);
				row.SetProperty(context.Result, (x) => x.Email);

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

				if (affiliateId > 0 && allAffiliateIds.Contains(affiliateId))
				{
					customer.AffiliateId = affiliateId;
				}

				if (isAdmin)
				{
					context.Result.AddInfo("Security. Ignored administrator role.", row.GetRowInfo(), "IsAdministrator");
				}

				UpsertRole(row, guestRole, isGuest);
				UpsertRole(row, registeredRole, isRegistered);
				UpsertRole(row, forumModeratorRole, isForumModerator);

				if (row.IsTransient)
				{
					_customerRepository.Insert(customer);
					lastInserted = customer;
				}
				else
				{
					_customerRepository.Update(customer);
					lastUpdated = customer;
				}
			}

			var num = _customerRepository.Context.SaveChanges();

			if (lastInserted != null)
			{
				_services.EventPublisher.EntityInserted(lastInserted);
			}

			if (lastUpdated != null)
			{
				_services.EventPublisher.EntityUpdated(lastUpdated);
			}

			return num;
		}

		public static string[] SupportedKeyFields
		{
			get
			{
				return new string[] { "Id", "CustomerGuid", "Email", "Username" };
			}
		}

		public static string[] DefaultKeyFields
		{
			get
			{
				return new string[] { "Email" };
			}
		}

		protected override void Import(IImportExecuteContext context)
		{
			var customer = _services.WorkContext.CurrentCustomer;
			var allowManagingCustomerRoles = _services.Permissions.Authorize(StandardPermissionProvider.ManageCustomerRoles, customer);

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
				_genericAttributeService.GetAttributes(SystemCustomerAttributeNames.CustomerNumber, _attributeKeyGroup).Select(x => x.Value), 
				StringComparer.OrdinalIgnoreCase);

			using (var scope = new DbContextScope(ctx: _services.DbContext, autoDetectChanges: false, proxyCreation: false, validateOnSave: false, autoCommit: false))
			{
				var segmenter = context.CreateSegmenter();

				Init(context, _dataExchangeSettings);

				context.Result.TotalRecords = segmenter.TotalRows;

				while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
				{
					var batch = segmenter.GetCurrentBatch<Customer>();

					_customerRepository.Context.DetachAll(false);

					context.SetProgress(segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

					// ===========================================================================
					// Process customers
					// ===========================================================================
					try
					{
						ProcessCustomers(context, batch, allAffiliateIds);
					}
					catch (Exception exception)
					{
						context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessCustomers");
					}

					// reduce batch to saved (valid) records.
					// No need to perform import operations on errored records.
					batch = batch.Where(x => x.Entity != null && !x.IsTransient).ToArray();

					// update result object
					context.Result.NewRecords += batch.Count(x => x.IsNew && !x.IsTransient);
					context.Result.ModifiedRecords += batch.Count(x => !x.IsNew && !x.IsTransient);

					// ===========================================================================
					// Process generic attributes
					// ===========================================================================
					try
					{
						ProcessGenericAttributes(context, batch, allCountries, allStateProvinces, allCustomerNumbers);
					}
					catch (Exception exception)
					{
						context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessGenericAttributes");
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
						catch (Exception exception)
						{
							context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessAvatars");
						}
					}

					// ===========================================================================
					// Process addresses
					// ===========================================================================
					if (segmenter.HasColumn("BillingAddress.LastName") || segmenter.HasColumn("ShippingAddress.LastName"))
					{
						try
						{
							ProcessAddresses(context, batch, allCountries, allStateProvinces);
						}
						catch (Exception exception)
						{
							context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessAddresses");
						}
					}
				}
			}
		}
	}
}
