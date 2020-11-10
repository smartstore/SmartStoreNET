using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Fakes;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Data.Caching;
using SmartStore.Services.Common;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;

namespace SmartStore.Services.Customers
{
    public partial class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<CustomerRoleMapping> _customerRoleMappingRepository;
        private readonly IRepository<GenericAttribute> _gaRepository;
        private readonly IRepository<RewardPointsHistory> _rewardPointsHistoryRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly Lazy<RewardPointsSettings> _rewardPointsSettings;
        private readonly ICommonServices _services;
        private readonly HttpContextBase _httpContext;
        private readonly IUserAgent _userAgent;
        private readonly CustomerSettings _customerSettings;
        private readonly Lazy<IGdprTool> _gdprTool;

        public CustomerService(
            IRepository<Customer> customerRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<CustomerRoleMapping> customerRoleMappingRepository,
            IRepository<GenericAttribute> gaRepository,
            IRepository<RewardPointsHistory> rewardPointsHistoryRepository,
            IRepository<ShoppingCartItem> shoppingCartItemRepository,
            IGenericAttributeService genericAttributeService,
            Lazy<RewardPointsSettings> rewardPointsSettings,
            ICommonServices services,
            HttpContextBase httpContext,
            IUserAgent userAgent,
            CustomerSettings customerSettings,
            Lazy<IGdprTool> gdprTool)
        {
            _customerRepository = customerRepository;
            _customerRoleRepository = customerRoleRepository;
            _customerRoleMappingRepository = customerRoleMappingRepository;
            _gaRepository = gaRepository;
            _rewardPointsHistoryRepository = rewardPointsHistoryRepository;
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _genericAttributeService = genericAttributeService;
            _rewardPointsSettings = rewardPointsSettings;
            _services = services;
            _httpContext = httpContext;
            _userAgent = userAgent;
            _customerSettings = customerSettings;
            _gdprTool = gdprTool;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }

        public ILogger Logger { get; set; }

        #region Customers

        public virtual IPagedList<Customer> SearchCustomers(CustomerSearchQuery q)
        {
            Guard.NotNull(q, nameof(q));

            var isOrdered = false;
            IQueryable<Customer> query = null;

            if (q.OnlyWithCart)
            {
                var cartItemQuery = _shoppingCartItemRepository.TableUntracked.Expand(x => x.Customer);

                if (q.CartType.HasValue)
                {
                    cartItemQuery = cartItemQuery.Where(x => x.ShoppingCartTypeId == (int)q.CartType.Value);
                }

                var groupQuery =
                    from sci in cartItemQuery
                    group sci by sci.CustomerId into grp
                    select grp
                        .OrderByDescending(x => x.CreatedOnUtc)
                        .Select(x => new
                        {
                            x.Customer,
                            x.CreatedOnUtc
                        })
                        .FirstOrDefault();

                // We have to sort again because of paging.
                query = groupQuery
                    .OrderByDescending(x => x.CreatedOnUtc)
                    .Select(x => x.Customer);

                isOrdered = true;
            }
            else
            {
                query = _customerRepository.Table;
            }

            if (q.Email.HasValue())
            {
                query = query.Where(c => c.Email.Contains(q.Email));
            }

            if (q.Username.HasValue())
            {
                query = query.Where(c => c.Username.Contains(q.Username));
            }

            if (q.CustomerNumber.HasValue())
            {
                query = query.Where(c => c.CustomerNumber.Contains(q.CustomerNumber));
            }

            if (q.AffiliateId.GetValueOrDefault() > 0)
            {
                query = query.Where(c => c.AffiliateId == q.AffiliateId.Value);
            }

            if (q.SearchTerm.HasValue())
            {
                if (_customerSettings.CompanyEnabled)
                {
                    query = query.Where(c => c.FullName.Contains(q.SearchTerm) || c.Company.Contains(q.SearchTerm));
                }
                else
                {
                    query = query.Where(c => c.FullName.Contains(q.SearchTerm));
                }
            }

            if (q.DayOfBirth.GetValueOrDefault() > 0)
            {
                query = query.Where(c => c.BirthDate.Value.Day == q.DayOfBirth.Value);
            }

            if (q.MonthOfBirth.GetValueOrDefault() > 0)
            {
                query = query.Where(c => c.BirthDate.Value.Month == q.MonthOfBirth.Value);
            }

            if (q.RegistrationFromUtc.HasValue)
            {
                query = query.Where(c => q.RegistrationFromUtc.Value <= c.CreatedOnUtc);
            }

            if (q.RegistrationToUtc.HasValue)
            {
                query = query.Where(c => q.RegistrationToUtc.Value >= c.CreatedOnUtc);
            }

            if (q.LastActivityFromUtc.HasValue)
            {
                query = query.Where(c => q.LastActivityFromUtc.Value <= c.LastActivityDateUtc);
            }

            if (q.CustomerRoleIds != null && q.CustomerRoleIds.Length > 0)
            {
                query = query.Where(c => c.CustomerRoleMappings.Select(rm => rm.CustomerRoleId).Intersect(q.CustomerRoleIds).Count() > 0);
            }

            if (q.Deleted.HasValue)
            {
                query = query.Where(c => c.Deleted == q.Deleted.Value);
            }

            if (q.Active.HasValue)
            {
                query = query.Where(c => c.Active == q.Active.Value);
            }

            if (q.IsSystemAccount.HasValue)
            {
                query = q.IsSystemAccount.Value == true
                    ? query.Where(c => !string.IsNullOrEmpty(c.SystemName))
                    : query.Where(c => string.IsNullOrEmpty(c.SystemName));
            }

            if (q.PasswordFormat.HasValue)
            {
                int passwordFormatId = (int)q.PasswordFormat.Value;
                query = query.Where(c => c.PasswordFormatId == passwordFormatId);
            }

            // Search by phone
            if (q.Phone.HasValue())
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.Phone &&
                        z.Attribute.Value.Contains(q.Phone))
                    .Select(z => z.Customer);
            }

            // Search by zip
            if (q.ZipPostalCode.HasValue())
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where(z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.ZipPostalCode &&
                        z.Attribute.Value.Contains(q.ZipPostalCode))
                    .Select(z => z.Customer);
            }

            if (!isOrdered)
            {
                query = query.OrderByDescending(c => c.CreatedOnUtc);
            }

            var customers = new PagedList<Customer>(query, q.PageIndex, q.PageSize);
            return customers;
        }

        public virtual IPagedList<Customer> GetAllCustomersByPasswordFormat(PasswordFormat passwordFormat)
        {
            var q = new CustomerSearchQuery
            {
                PasswordFormat = passwordFormat,
                PageIndex = 0,
                PageSize = 500
            };

            var customers = SearchCustomers(q);
            return customers;
        }

        public virtual IPagedList<Customer> GetOnlineCustomers(DateTime lastActivityFromUtc, int[] customerRoleIds, int pageIndex, int pageSize)
        {
            var q = new CustomerSearchQuery
            {
                LastActivityFromUtc = lastActivityFromUtc,
                CustomerRoleIds = customerRoleIds,
                IsSystemAccount = false,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            var customers = SearchCustomers(q);

            customers.AlterQuery(x => x.OrderByDescending(c => c.LastActivityDateUtc));

            return customers;
        }

        public virtual void DeleteCustomer(Customer customer)
        {
            Guard.NotNull(customer, nameof(customer));

            if (customer.IsSystemAccount)
                throw new SmartException(string.Format("System customer account ({0}) cannot not be deleted", customer.SystemName));

            // Soft delete
            customer.Deleted = true;

            // Anonymize IP addresses
            var language = customer.GetLanguage();

            _gdprTool.Value.AnonymizeData(customer, x => x.LastIpAddress, IdentifierDataType.IpAddress, language);

            foreach (var post in customer.ForumPosts)
            {
                _gdprTool.Value.AnonymizeData(post, x => x.IPAddress, IdentifierDataType.IpAddress, language);
            }

            // Customer Content
            foreach (var item in customer.CustomerContent)
            {
                _gdprTool.Value.AnonymizeData(item, x => x.IpAddress, IdentifierDataType.IpAddress, language);
            }

            UpdateCustomer(customer);
        }

        public virtual Customer GetCustomerById(int customerId)
        {
            if (customerId == 0)
                return null;

            // var customer = _customerRepository.GetById(customerId);
            var customer = IncludeRoles(_customerRepository.Table).SingleOrDefault(x => x.Id == customerId);

            return customer;
        }

        private IQueryable<Customer> IncludeRoles(IQueryable<Customer> query)
        {
            return query
              .Expand(x => x.CustomerRoleMappings.Select(y => y.CustomerRole));

            /// The generated SQL is way too heavy (!). Discard later (??)
            //return query
            //	.Expand(x => x.ShoppingCartItems.Select(y => y.BundleItem))
            //  .Expand(x => x.ShoppingCartItems.Select(y => y.Product.AppliedDiscounts.Select(z => z.RuleSets)));
        }

        public virtual IList<Customer> GetCustomersByIds(int[] customerIds)
        {
            if (customerIds == null || customerIds.Length == 0)
                return new List<Customer>();

            var query = from c in _customerRepository.Table
                        where customerIds.Contains(c.Id)
                        select c;

            var customers = query.ToList();

            // sort by passed identifier sequence
            return customers.OrderBySequence(customerIds).ToList();
        }

        public virtual IList<Customer> GetSystemAccountCustomers()
        {
            return _customerRepository.Table.Where(x => x.IsSystemAccount).ToList();
        }

        public virtual Customer GetCustomerByGuid(Guid customerGuid)
        {
            if (customerGuid == Guid.Empty)
                return null;

            var query = from c in IncludeRoles(_customerRepository.Table)
                        where c.CustomerGuid == customerGuid
                        orderby c.Id
                        select c;

            var customer = query.FirstOrDefault();
            return customer;
        }

        public virtual Customer GetCustomerByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var query = from c in IncludeRoles(_customerRepository.Table)
                        orderby c.Id
                        where c.Email == email
                        select c;

            var customer = query.FirstOrDefault();
            return customer;
        }

        public virtual Customer GetCustomerBySystemName(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var query = from c in _customerRepository.Table
                        orderby c.Id
                        where c.SystemName == systemName
                        select c;

            var customer = query.FirstOrDefault();
            return customer;
        }

        public virtual Customer GetCustomerByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            var query = from c in IncludeRoles(_customerRepository.Table)
                        orderby c.Id
                        where c.Username == username
                        select c;

            var customer = query.FirstOrDefault();
            return customer;
        }

        public virtual Customer InsertGuestCustomer(Guid? customerGuid = null)
        {
            var customer = new Customer
            {
                CustomerGuid = customerGuid ?? Guid.NewGuid(),
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };

            // Add to 'Guests' role
            var guestRole = GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests);
            if (guestRole == null)
            {
                throw new SmartException("'Guests' role could not be loaded");
            }

            using (new DbContextScope(autoCommit: true))
            {
                // Ensure that entities are saved to db in any case
                customer.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = guestRole.Id });
                _customerRepository.Insert(customer);

                var clientIdent = _services.WebHelper.GetClientIdent();
                if (clientIdent.HasValue())
                {
                    _genericAttributeService.SaveAttribute(customer, "ClientIdent", clientIdent);
                }
            }

            //Logger.DebugFormat("Guest account created for anonymous visitor. Id: {0}, ClientIdent: {1}", customer.CustomerGuid, clientIdent ?? "n/a");

            return customer;
        }

        public virtual Customer FindGuestCustomerByClientIdent(string clientIdent = null, int maxAgeSeconds = 60)
        {
            if (_httpContext.IsFakeContext() || _userAgent.IsBot || _userAgent.IsPdfConverter)
            {
                return null;
            }

            using (_services.Chronometer.Step("FindGuestCustomerByClientIdent"))
            {
                clientIdent = clientIdent.NullEmpty() ?? _services.WebHelper.GetClientIdent();
                if (clientIdent.IsEmpty())
                {
                    return null;
                }

                var dateFrom = DateTime.UtcNow.AddSeconds(maxAgeSeconds * -1);

                IQueryable<Customer> query;
                if (DataSettings.Current.IsSqlServer)
                {
                    query = from a in _gaRepository.Table
                            join c in _customerRepository.Table on a.EntityId equals c.Id into Customers
                            from c in Customers.DefaultIfEmpty()
                            where c.LastActivityDateUtc >= dateFrom
                                && c.Username == null
                                && c.Email == null
                                && a.KeyGroup == "Customer"
                                && a.Key == "ClientIdent"
                                && a.Value == clientIdent
                            select c;
                }
                else
                {
                    query = from a in _gaRepository.Table
                            join c in _customerRepository.Table on a.EntityId equals c.Id into Customers
                            from c in Customers.DefaultIfEmpty()
                            where c.LastActivityDateUtc >= dateFrom
                                && c.Username == null
                                && c.Email == null
                                && a.KeyGroup == "Customer"
                                && a.Key == "ClientIdent"
                                && a.Value.Contains(clientIdent) // SQLCE doesn't like ntext in WHERE clauses
                            select c;
                }

                return query.FirstOrDefault();
            }
        }

        public virtual void InsertCustomer(Customer customer)
        {
            Guard.NotNull(customer, nameof(customer));

            /// Validate unique user. <see cref="ICustomerRegistrationService.RegisterCustomer(CustomerRegistrationRequest)"/>
            if (customer.Email.HasValue() && GetCustomerByEmail(customer.Email) != null)
            {
                throw new SmartException(T("Account.Register.Errors.EmailAlreadyExists"));
            }

            if (customer.Username.HasValue() &&
                _customerSettings.CustomerLoginType != CustomerLoginType.Email &&
                GetCustomerByUsername(customer.Username) != null)
            {
                throw new SmartException(T("Account.Register.Errors.UsernameAlreadyExists"));
            }

            _customerRepository.Insert(customer);
        }

        public virtual void UpdateCustomer(Customer customer)
        {
            Guard.NotNull(customer, nameof(customer));

            _customerRepository.Update(customer);
        }

        public virtual void ResetCheckoutData(
            Customer customer,
            int storeId,
            bool clearCouponCodes = false,
            bool clearCheckoutAttributes = false,
            bool clearRewardPoints = false,
            bool clearShippingMethod = true,
            bool clearPaymentMethod = true,
            bool clearCreditBalance = false)
        {
            Guard.NotNull(customer, nameof(customer));

            if (clearCouponCodes)
            {
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.DiscountCouponCode, null);
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.GiftCardCouponCodes, null);
            }

            if (clearCheckoutAttributes)
            {
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.CheckoutAttributes, null);
            }

            if (clearRewardPoints)
            {
                _genericAttributeService.SaveAttribute<bool>(customer, SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, false, storeId);
            }

            if (clearCreditBalance)
            {
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.UseCreditBalanceDuringCheckout, decimal.Zero, storeId);
            }

            if (clearShippingMethod)
            {
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.SelectedShippingOption, null, storeId);
                _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.OfferedShippingOptions, null, storeId);
            }

            if (clearPaymentMethod)
            {
                _genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, null, storeId);
            }

            UpdateCustomer(customer);
        }

        public virtual int DeleteGuestCustomers(
            DateTime? registrationFrom,
            DateTime? registrationTo,
            bool onlyWithoutShoppingCart)
        {
            var genericAttributesSql = @"
DELETE TOP(50000) [g]
  FROM [dbo].[GenericAttribute] AS [g]
  LEFT OUTER JOIN [dbo].[Customer] AS [c] ON c.Id = g.EntityId
  LEFT OUTER JOIN [dbo].[Order] AS [o] ON c.Id = o.CustomerId
  LEFT OUTER JOIN [dbo].[CustomerContent] AS [cc] ON c.Id = cc.CustomerId
  LEFT OUTER JOIN [dbo].[Forums_PrivateMessage] AS [pm] ON c.Id = pm.ToCustomerId
  LEFT OUTER JOIN [dbo].[Forums_Post] AS [fp] ON c.Id = fp.CustomerId
  LEFT OUTER JOIN [dbo].[Forums_Topic] AS [ft] ON c.Id = ft.CustomerId
  WHERE g.KeyGroup = 'Customer' AND c.Username IS Null AND c.Email IS NULL AND c.IsSystemAccount = 0{0}
	AND (NOT EXISTS (SELECT 1 AS [C1] FROM [dbo].[Order] AS [o1] WHERE c.Id = o1.CustomerId ))
	AND (NOT EXISTS (SELECT 1 AS [C1] FROM [dbo].[CustomerContent] AS [cc1] WHERE c.Id = cc1.CustomerId ))
	AND (NOT EXISTS (SELECT 1 AS [C1] FROM [dbo].[Forums_PrivateMessage] AS [pm1] WHERE c.Id = pm1.ToCustomerId ))
	AND (NOT EXISTS (SELECT 1 AS [C1] FROM [dbo].[Forums_Post] AS [fp1] WHERE c.Id = fp1.CustomerId ))
	AND (NOT EXISTS (SELECT 1 AS [C1] FROM [dbo].[Forums_Topic] AS [ft1] WHERE c.Id = ft1.CustomerId ))
";

            var guestCustomersSql = @"
DELETE TOP(20000) [c]
  FROM [dbo].[Customer] AS [c]
  LEFT OUTER JOIN [dbo].[Order] AS [o] ON c.Id = o.CustomerId
  LEFT OUTER JOIN [dbo].[CustomerContent] AS [cc] ON c.Id = cc.CustomerId
  LEFT OUTER JOIN [dbo].[Forums_PrivateMessage] AS [pm] ON c.Id = pm.ToCustomerId
  LEFT OUTER JOIN [dbo].[Forums_Post] AS [fp] ON c.Id = fp.CustomerId
  LEFT OUTER JOIN [dbo].[Forums_Topic] AS [ft] ON c.Id = ft.CustomerId
  WHERE c.Username IS Null AND c.Email IS NULL AND c.IsSystemAccount = 0{0}
	AND (NOT EXISTS (SELECT 1 AS x FROM [dbo].[Order] AS [o1] WHERE c.Id = o1.CustomerId ))
	AND (NOT EXISTS (SELECT 1 AS x FROM [dbo].[CustomerContent] AS [cc1] WHERE c.Id = cc1.CustomerId ))
	AND (NOT EXISTS (SELECT 1 AS x FROM [dbo].[Forums_PrivateMessage] AS [pm1] WHERE c.Id = pm1.ToCustomerId ))
	AND (NOT EXISTS (SELECT 1 AS x FROM [dbo].[Forums_Post] AS [fp1] WHERE c.Id = fp1.CustomerId ))
	AND (NOT EXISTS (SELECT 1 AS x FROM [dbo].[Forums_Topic] AS [ft1] WHERE c.Id = ft1.CustomerId ))
";

            var ctx = _customerRepository.Context;
            var paramClauses = new StringBuilder();
            var parameters = new List<object>();
            var numberOfDeletedCustomers = 0;
            var numberOfDeletedAttributes = 0;
            var pIndex = 0;

            if (registrationFrom.HasValue)
            {
                paramClauses.AppendFormat(" AND @p{0} <= c.CreatedOnUtc", pIndex++);
                parameters.Add(registrationFrom.Value);
            }
            if (registrationTo.HasValue)
            {
                paramClauses.AppendFormat(" AND @p{0} >= c.CreatedOnUtc", pIndex++);
                parameters.Add(registrationTo.Value);
            }
            if (onlyWithoutShoppingCart)
            {
                paramClauses.Append(" AND (NOT EXISTS (SELECT 1 AS [C1] FROM [dbo].[ShoppingCartItem] AS [sci] WHERE c.Id = sci.CustomerId ))");
            }

            genericAttributesSql = genericAttributesSql.FormatInvariant(paramClauses.ToString());
            guestCustomersSql = guestCustomersSql.FormatInvariant(paramClauses.ToString());


            // Delete generic attributes.
            while (true)
            {
                var numDeleted = ctx.ExecuteSqlCommand(genericAttributesSql, false, null, parameters.ToArray());
                if (numDeleted <= 0)
                {
                    break;
                }

                numberOfDeletedAttributes += numDeleted;
            }

            // Delete guest customers.
            while (true)
            {
                var numDeleted = ctx.ExecuteSqlCommand(guestCustomersSql, false, null, parameters.ToArray());
                if (numDeleted <= 0)
                {
                    break;
                }

                numberOfDeletedCustomers += numDeleted;
            }

            Debug.WriteLine("Deleted {0} guest customers including {1} generic attributes.", numberOfDeletedCustomers, numberOfDeletedAttributes);

            return numberOfDeletedCustomers;
        }

        #endregion

        #region Customer roles

        public virtual void DeleteCustomerRole(CustomerRole role)
        {
            Guard.NotNull(role, nameof(role));

            if (role.IsSystemRole)
                throw new SmartException("System role could not be deleted");

            var roleId = role.Id;

            _customerRoleRepository.Delete(role);

            _services.Cache.RemoveByPattern(PermissionService.PERMISSION_TREE_KEY.FormatInvariant(roleId));
        }

        public virtual CustomerRole GetCustomerRoleById(int roleId)
        {
            if (roleId == 0)
                return null;

            return _customerRoleRepository.GetById(roleId);
        }

        public virtual CustomerRole GetCustomerRoleBySystemName(string systemName)
        {
            if (String.IsNullOrWhiteSpace(systemName))
                return null;

            var query = from cr in _customerRoleRepository.Table
                        orderby cr.Id
                        where cr.SystemName == systemName
                        select cr;

            var customerRole = query.FirstOrDefaultCached();
            return customerRole;
        }

        public virtual IPagedList<CustomerRole> GetAllCustomerRoles(bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from cr in _customerRoleRepository.Table
                        orderby cr.Name
                        where (showHidden || cr.Active)
                        select cr;

            return new PagedList<CustomerRole>(query, pageIndex, pageSize);
        }

        public virtual void InsertCustomerRole(CustomerRole role)
        {
            Guard.NotNull(role, nameof(role));

            _customerRoleRepository.Insert(role);
        }

        public virtual void UpdateCustomerRole(CustomerRole role)
        {
            Guard.NotNull(role, nameof(role));

            _customerRoleRepository.Update(role);

            _services.Cache.RemoveByPattern(PermissionService.PERMISSION_TREE_KEY.FormatInvariant(role.Id));
        }

        #endregion

        #region Customer role mappings

        public virtual CustomerRoleMapping GetCustomerRoleMappingById(int mappingId)
        {
            if (mappingId == 0)
            {
                return null;
            }

            return _customerRoleMappingRepository.GetById(mappingId);
        }

        public virtual IPagedList<CustomerRoleMapping> GetCustomerRoleMappings(
            int[] customerIds,
            int[] customerRoleIds,
            bool? isSystemMapping,
            int pageIndex,
            int pageSize,
            bool withCustomers = true)
        {
            var query = _customerRoleMappingRepository.TableUntracked;

            if (withCustomers)
            {
                query = query.Include(x => x.Customer);
            }

            if (customerIds?.Any() ?? false)
            {
                query = query.Where(x => customerIds.Contains(x.CustomerId));
            }
            if (customerRoleIds?.Any() ?? false)
            {
                query = query.Where(x => customerRoleIds.Contains(x.CustomerRoleId));
            }
            if (isSystemMapping.HasValue)
            {
                query = query.Where(x => x.IsSystemMapping == isSystemMapping.Value);
            }

            query = query.Where(x => !x.Customer.Deleted);

            if (withCustomers)
            {
                query = query
                    .OrderBy(x => x.IsSystemMapping)
                    .ThenByDescending(x => x.Customer.CreatedOnUtc);
            }
            else
            {
                query = query.OrderBy(x => x.IsSystemMapping);
            }

            var mappings = new PagedList<CustomerRoleMapping>(query, pageIndex, pageSize);
            return mappings;
        }

        public virtual void InsertCustomerRoleMapping(CustomerRoleMapping mapping)
        {
            Guard.NotNull(mapping, nameof(mapping));

            _customerRoleMappingRepository.Insert(mapping);
        }

        public virtual void UpdateCustomerRoleMapping(CustomerRoleMapping mapping)
        {
            Guard.NotNull(mapping, nameof(mapping));

            _customerRoleMappingRepository.Update(mapping);
        }

        public virtual void DeleteCustomerRoleMapping(CustomerRoleMapping mapping)
        {
            if (mapping != null)
            {
                _customerRoleMappingRepository.Delete(mapping);
            }
        }

        #endregion

        #region Reward points

        public virtual void RewardPointsForProductReview(Customer customer, Product product, bool add)
        {
            var rpSettings = _rewardPointsSettings.Value;

            if (rpSettings.Enabled && rpSettings.PointsForProductReview > 0)
            {
                string message = T(add ? "RewardPoints.Message.EarnedForProductReview" : "RewardPoints.Message.ReducedForProductReview", product.GetLocalized(x => x.Name));

                customer.AddRewardPointsHistoryEntry(rpSettings.PointsForProductReview * (add ? 1 : -1), message);

                UpdateCustomer(customer);
            }
        }

        public virtual Multimap<int, RewardPointsHistory> GetRewardPointsHistoriesByCustomerIds(int[] customerIds)
        {
            Guard.NotNull(customerIds, nameof(customerIds));

            var query =
                from x in _rewardPointsHistoryRepository.TableUntracked
                where customerIds.Contains(x.CustomerId)
                select x;

            var map = query
                .OrderBy(x => x.CustomerId)
                .ThenByDescending(x => x.CreatedOnUtc)
                .ThenByDescending(x => x.Id)
                .ToList()
                .ToMultimap(x => x.CustomerId, x => x);

            return map;
        }

        #endregion
    }
}