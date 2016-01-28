using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Events;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Common;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Security;

namespace SmartStore.Services.Customers.Importer
{
	public class CustomerImporter : IEntityImporter
	{
		private const string _attributeKeyGroup = "Customer";

		private readonly ICommonServices _services;
		private readonly IRepository<Customer> _customerRepository;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly ICustomerService _customerService;
		private readonly IAffiliateService _affiliateService;
		private readonly ICountryService _countryService;
		private readonly IStateProvinceService _stateProvinceService;
		private readonly CustomerSettings _customerSettings;
		private readonly DateTimeSettings _dateTimeSettings;
		private readonly ForumSettings _forumSettings;

		public CustomerImporter(
			ICommonServices services,
			IRepository<Customer> customerRepository,
			IGenericAttributeService genericAttributeService,
			ICustomerService customerService,
			IAffiliateService affiliateService,
			ICountryService countryService,
			IStateProvinceService stateProvinceService,
			CustomerSettings customerSettings,
			DateTimeSettings dateTimeSettings,
			ForumSettings forumSettings)
		{
			_services = services;
			_customerRepository = customerRepository;
			_genericAttributeService = genericAttributeService;
			_customerService = customerService;
			_affiliateService = affiliateService;
			_countryService = countryService;
			_stateProvinceService = stateProvinceService;
			_customerSettings = customerSettings;
			_dateTimeSettings = dateTimeSettings;
			_forumSettings = forumSettings;
		}

		private void SaveAttribute(ImportRow<Customer> row, string key)
		{
			_genericAttributeService.SaveAttribute(row.Entity.Id, key, _attributeKeyGroup, row.GetDataValue<string>(key));
		}

		private void UpsertRole(ImportRow<Customer> row, CustomerRole role, string roleSystemName, bool value)
		{
			if (role != null)
			{
				var hasRole = row.Entity.CustomerRoles.Any(x => x.SystemName == roleSystemName);

				if (value && !hasRole)
					row.Entity.CustomerRoles.Add(role);
				else if (!value && hasRole)
					row.Entity.CustomerRoles.Remove(role);
			}
		}

		private int ProcessGenericAttributes(IImportExecuteContext context,
			ImportRow<Customer>[] batch,
			List<int> allCountryIds,
			List<int> allStateProvinceIds,
			List<string> allCustomerNumbers)
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
					SaveAttribute(row, SystemCustomerAttributeNames.DateOfBirth);

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
					SaveAttribute(row, SystemCustomerAttributeNames.CountryId);

				if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
					SaveAttribute(row, SystemCustomerAttributeNames.StateProvinceId);

				if (_customerSettings.PhoneEnabled)
					SaveAttribute(row, SystemCustomerAttributeNames.Phone);

				if (_customerSettings.FaxEnabled)
					SaveAttribute(row, SystemCustomerAttributeNames.Fax);

				if (_customerSettings.AllowCustomersToUploadAvatars)
					SaveAttribute(row, SystemCustomerAttributeNames.AvatarPictureId);

				if (_forumSettings.ForumsEnabled)
					SaveAttribute(row, SystemCustomerAttributeNames.ForumPostCount);

				if (_forumSettings.SignaturesEnabled)
					SaveAttribute(row, SystemCustomerAttributeNames.Signature);

				var countryId = row.GetDataValue<int>("CountryId");
				var stateProvinceId = row.GetDataValue<int>("StateProvinceId");

				if (countryId != 0 && allCountryIds.Contains(countryId))
				{
					_genericAttributeService.SaveAttribute(row.Entity.Id, SystemCustomerAttributeNames.CountryId, _attributeKeyGroup, countryId);
				}

				if (stateProvinceId != 0 && allStateProvinceIds.Contains(stateProvinceId))
				{
					_genericAttributeService.SaveAttribute(row.Entity.Id, SystemCustomerAttributeNames.StateProvinceId, _attributeKeyGroup, stateProvinceId);
				}

				string customerNumber = null;

				if (_customerSettings.CustomerNumberMethod == CustomerNumberMethod.AutomaticallySet)
					customerNumber = row.Entity.Id.ToString();
				else
					customerNumber = row.GetDataValue<string>("CustomerNumber");

				if (customerNumber.IsEmpty() || !allCustomerNumbers.Any(x => x.IsCaseInsensitiveEqual(customerNumber)))
				{
					_genericAttributeService.SaveAttribute(row.Entity.Id, SystemCustomerAttributeNames.CustomerNumber, _attributeKeyGroup, customerNumber);

					if (!customerNumber.IsEmpty())
						allCustomerNumbers.Add(customerNumber);
				}
			}

			var num = _customerRepository.Context.SaveChanges();

			return num;
		}

		private int ProcessCustomers(IImportExecuteContext context,
			ImportRow<Customer>[] batch,
			DateTime utcNow,
			List<int> allAffiliateIds,
			IList<CustomerRole> allCustomerRoles)
		{
			_customerRepository.AutoCommitEnabled = true;

			Customer lastInserted = null;
			Customer lastUpdated = null;

			var guestRole = allCustomerRoles.FirstOrDefault(x => x.SystemName == SystemCustomerRoleNames.Guests);
			var registeredRole = allCustomerRoles.FirstOrDefault(x => x.SystemName == SystemCustomerRoleNames.Registered);
			var adminRole = allCustomerRoles.FirstOrDefault(x => x.SystemName == SystemCustomerRoleNames.Administrators);
			var forumModeratorRole = allCustomerRoles.FirstOrDefault(x => x.SystemName == SystemCustomerRoleNames.ForumModerators);

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

				row.Initialize(customer, email.HasValue() ? email : id.ToString());

				row.SetProperty(context.Result, customer, (x) => x.CustomerGuid);
				row.SetProperty(context.Result, customer, (x) => x.Username);
				row.SetProperty(context.Result, customer, (x) => x.Email);
				row.SetProperty(context.Result, customer, (x) => x.Password);
				row.SetProperty(context.Result, customer, (x) => x.PasswordFormatId);
				row.SetProperty(context.Result, customer, (x) => x.PasswordSalt);
				row.SetProperty(context.Result, customer, (x) => x.AdminComment);
				row.SetProperty(context.Result, customer, (x) => x.IsTaxExempt);
				row.SetProperty(context.Result, customer, (x) => x.Active);
				row.SetProperty(context.Result, customer, (x) => x.IsSystemAccount);
				row.SetProperty(context.Result, customer, (x) => x.SystemName);
				row.SetProperty(context.Result, customer, (x) => x.LastIpAddress);
				row.SetProperty(context.Result, customer, (x) => x.LastLoginDateUtc);
				row.SetProperty(context.Result, customer, (x) => x.LastActivityDateUtc);

				row.SetProperty(context.Result, customer, (x) => x.CreatedOnUtc, utcNow);
				row.SetProperty(context.Result, customer, (x) => x.LastActivityDateUtc, utcNow);

				if (affiliateId > 0 && allAffiliateIds.Contains(affiliateId))
				{
					customer.AffiliateId = affiliateId;
				}

				UpsertRole(row, guestRole, SystemCustomerRoleNames.Guests, isGuest);
				UpsertRole(row, registeredRole, SystemCustomerRoleNames.Registered, isRegistered);
				UpsertRole(row, adminRole, SystemCustomerRoleNames.Administrators, isAdmin);
				UpsertRole(row, forumModeratorRole, SystemCustomerRoleNames.ForumModerators, isForumModerator);

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
				_services.EventPublisher.EntityInserted(lastInserted);
			if (lastUpdated != null)
				_services.EventPublisher.EntityUpdated(lastUpdated);

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
				return new string[] { "Id", "CustomerGuid" };
			}
		}

		public void Execute(IImportExecuteContext context)
		{
			var utcNow = DateTime.UtcNow;
			var customer = _customerService.GetCustomerById(context.CustomerId);
			var allowManagingCustomerRoles = _services.Permissions.Authorize(StandardPermissionProvider.ManageCustomerRoles, customer);

			var allCustomerRoles = _customerService.GetAllCustomerRoles(true);

			var allAffiliateIds = _affiliateService.GetAllAffiliates(true)
				.Select(x => x.Id)
				.ToList();

			var allCountryIds = _countryService.GetAllCountries(true)
				.Select(x => x.Id)
				.ToList();

			var allStateProvinceIds = _stateProvinceService.GetAllStateProvinces(true)
				.Select(x => x.Id)
				.ToList();

			var allCustomerNumbers = _genericAttributeService.GetAttributes(SystemCustomerAttributeNames.CustomerNumber, _attributeKeyGroup)
				.Select(x => x.Value)
				.ToList();

			using (var scope = new DbContextScope(ctx: _services.DbContext, autoDetectChanges: false, proxyCreation: false, validateOnSave: false, autoCommit: false))
			{
				var segmenter = context.GetSegmenter<Customer>();

				context.Result.TotalRecords = segmenter.TotalRows;

				while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
				{
					var batch = segmenter.CurrentBatch;

					_customerRepository.Context.DetachAll(false);

					context.SetProgress(segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

					try
					{
						ProcessCustomers(context, batch, utcNow, allAffiliateIds, allCustomerRoles);
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

					try
					{
						ProcessGenericAttributes(context, batch, allCountryIds, allStateProvinceIds, allCustomerNumbers);
					}
					catch (Exception exception)
					{
						context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessGenericAttributes");
					}
				}
			}
		}
	}
}
