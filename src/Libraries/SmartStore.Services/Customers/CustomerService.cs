using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Core.Fakes;
using SmartStore.Data.Caching;
using SmartStore.Services.Common;
using SmartStore.Services.Localization;
using SmartStore.Core.Logging;

namespace SmartStore.Services.Customers
{
	public partial class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<GenericAttribute> _gaRepository;
		private readonly IRepository<RewardPointsHistory> _rewardPointsHistoryRepository;
        private readonly IGenericAttributeService _genericAttributeService;
		private readonly RewardPointsSettings _rewardPointsSettings;
		private readonly ICommonServices _services;
		private readonly HttpContextBase _httpContext;
		private readonly IUserAgent _userAgent;

		public CustomerService(
            IRepository<Customer> customerRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<GenericAttribute> gaRepository,
			IRepository<RewardPointsHistory> rewardPointsHistoryRepository,
            IGenericAttributeService genericAttributeService,
			RewardPointsSettings rewardPointsSettings,
			ICommonServices services,
			HttpContextBase httpContext,
			IUserAgent userAgent)
        {
            this._customerRepository = customerRepository;
            this._customerRoleRepository = customerRoleRepository;
            this._gaRepository = gaRepository;
			this._rewardPointsHistoryRepository = rewardPointsHistoryRepository;
            this._genericAttributeService = genericAttributeService;
			this._rewardPointsSettings = rewardPointsSettings;
			this._services = services;
			this._httpContext = httpContext;
			this._userAgent = userAgent;

			T = NullLocalizer.Instance;
			Logger = NullLogger.Instance;
        }

		public Localizer T { get; set; }

		public ILogger Logger { get; set; }

		#region Customers

		public virtual IPagedList<Customer> GetAllCustomers(
			DateTime? registrationFrom,
            DateTime? registrationTo, 
			int[] customerRoleIds, 
			string email, 
			string username,
            string firstName, 
			string lastName, 
			int dayOfBirth, 
			int monthOfBirth,
            string company, 
			string phone, 
			string zipPostalCode,
            bool loadOnlyWithShoppingCart, 
			ShoppingCartType? sct, 
			int pageIndex, 
			int pageSize)
        {
            var query = _customerRepository.Table;
            if (registrationFrom.HasValue)
                query = query.Where(c => registrationFrom.Value <= c.CreatedOnUtc);
            if (registrationTo.HasValue)
                query = query.Where(c => registrationTo.Value >= c.CreatedOnUtc);
            query = query.Where(c => !c.Deleted);
            if (customerRoleIds != null && customerRoleIds.Length > 0)
                query = query.Where(c => c.CustomerRoles.Select(cr => cr.Id).Intersect(customerRoleIds).Count() > 0);
            if (!String.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!String.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username.Contains(username));
            if (!String.IsNullOrWhiteSpace(firstName))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.FirstName &&
                        z.Attribute.Value.Contains(firstName)))
                    .Select(z => z.Customer);
            }
            if (!String.IsNullOrWhiteSpace(lastName))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.LastName &&
                        z.Attribute.Value.Contains(lastName)))
                    .Select(z => z.Customer);
            }
            //date of birth is stored as a string into database.
            //we also know that date of birth is stored in the following format YYYY-MM-DD (for example, 1983-02-18).
            //so let's search it as a string
            if (dayOfBirth > 0 && monthOfBirth > 0)
            {
                //both are specified
                string dateOfBirthStr = monthOfBirth.ToString("00", CultureInfo.InvariantCulture) + "-" + dayOfBirth.ToString("00", CultureInfo.InvariantCulture);
                //EndsWith is not supported by SQL Server Compact
                //so let's use the following workaround http://social.msdn.microsoft.com/Forums/is/sqlce/thread/0f810be1-2132-4c59-b9ae-8f7013c0cc00
                
                //we also cannot use Length function in SQL Server Compact (not supported in this context)
                //z.Attribute.Value.Length - dateOfBirthStr.Length = 5
                //dateOfBirthStr.Length = 5
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.DateOfBirth &&
                        z.Attribute.Value.Substring(5, 5) == dateOfBirthStr))
                    .Select(z => z.Customer);
            }
            else if (dayOfBirth > 0)
            {
                //only day is specified
                string dateOfBirthStr = dayOfBirth.ToString("00", CultureInfo.InvariantCulture);
                //EndsWith is not supported by SQL Server Compact
                //so let's use the following workaround http://social.msdn.microsoft.com/Forums/is/sqlce/thread/0f810be1-2132-4c59-b9ae-8f7013c0cc00
                
                //we also cannot use Length function in SQL Server Compact (not supported in this context)
                //z.Attribute.Value.Length - dateOfBirthStr.Length = 8
                //dateOfBirthStr.Length = 2
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.DateOfBirth &&
                        z.Attribute.Value.Substring(8, 2) == dateOfBirthStr))
                    .Select(z => z.Customer);
            }
            else if (monthOfBirth > 0)
            {
                //only month is specified
                string dateOfBirthStr = "-" + monthOfBirth.ToString("00", CultureInfo.InvariantCulture) + "-";
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.DateOfBirth &&
                        z.Attribute.Value.Contains(dateOfBirthStr)))
                    .Select(z => z.Customer);
            }
            //search by company
            if (!String.IsNullOrWhiteSpace(company))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.Company &&
                        z.Attribute.Value.Contains(company)))
                    .Select(z => z.Customer);
            }
            //search by phone
            if (!String.IsNullOrWhiteSpace(phone))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.Phone &&
                        z.Attribute.Value.Contains(phone)))
                    .Select(z => z.Customer);
            }
            //search by zip
            if (!String.IsNullOrWhiteSpace(zipPostalCode))
            {
                query = query
                    .Join(_gaRepository.Table, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                    .Where((z => z.Attribute.KeyGroup == "Customer" &&
                        z.Attribute.Key == SystemCustomerAttributeNames.ZipPostalCode &&
                        z.Attribute.Value.Contains(zipPostalCode)))
                    .Select(z => z.Customer);
            }

            if (loadOnlyWithShoppingCart)
            {
                int? sctId = null;
                if (sct.HasValue)
                    sctId = (int)sct.Value;

                query = sct.HasValue ?
                    query.Where(c => c.ShoppingCartItems.Where(x => x.ShoppingCartTypeId == sctId).Count() > 0) :
                    query.Where(c => c.ShoppingCartItems.Count() > 0);
            }
            
            query = query.OrderByDescending(c => c.CreatedOnUtc);

            var customers = new PagedList<Customer>(query, pageIndex, pageSize);
            return customers;
        }

        public virtual IPagedList<Customer> GetAllCustomers(int affiliateId, int pageIndex, int pageSize)
        {
            var query = _customerRepository.Table;
            query = query.Where(c => !c.Deleted);
			query = query.Where(c => c.AffiliateId == affiliateId);
            query = query.OrderByDescending(c => c.CreatedOnUtc);

            var customers = new PagedList<Customer>(query, pageIndex, pageSize);
            return customers;
        }

        public virtual IList<Customer> GetAllCustomersByPasswordFormat(PasswordFormat passwordFormat)
        {
            int passwordFormatId = (int)passwordFormat;

            var query = _customerRepository.Table;
            query = query.Where(c => c.PasswordFormatId == passwordFormatId);
            query = query.OrderByDescending(c => c.CreatedOnUtc);
            var customers = query.ToList();
            return customers;
        }

        public virtual IPagedList<Customer> GetOnlineCustomers(DateTime lastActivityFromUtc,
            int[] customerRoleIds, int pageIndex, int pageSize)
        {
            var query = _customerRepository.Table;
            query = query.Where(c => lastActivityFromUtc <= c.LastActivityDateUtc);
            query = query.Where(c => !c.Deleted);
            if (customerRoleIds != null && customerRoleIds.Length > 0)
                query = query.Where(c => c.CustomerRoles.Select(cr => cr.Id).Intersect(customerRoleIds).Count() > 0);
            
            query = query.OrderByDescending(c => c.LastActivityDateUtc);
            var customers = new PagedList<Customer>(query, pageIndex, pageSize);
            return customers;
        }

        public virtual void DeleteCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (customer.IsSystemAccount)
                throw new SmartException(string.Format("System customer account ({0}) could not be deleted", customer.SystemName));

            customer.Deleted = true;
            UpdateCustomer(customer);
        }

        public virtual Customer GetCustomerById(int customerId)
        {
            if (customerId == 0)
                return null;
            
            // var customer = _customerRepository.GetById(customerId);
			var customer = IncludeShoppingCart(_customerRepository.Table).SingleOrDefault(x => x.Id == customerId);

			return customer;
        }

		private IQueryable<Customer> IncludeShoppingCart(IQueryable<Customer> query)
		{
			return query
				.Expand(x => x.ShoppingCartItems.Select(y => y.BundleItem))
				.Expand(x => x.ShoppingCartItems.Select(y => y.Product.AppliedDiscounts.Select(z => z.DiscountRequirements)));
		}

        public virtual IList<Customer> GetCustomersByIds(int[] customerIds)
        {
            if (customerIds == null || customerIds.Length == 0)
                return new List<Customer>();

            var query = from c in _customerRepository.Table
                        where customerIds.Contains(c.Id)
                        select c;
            var customers = query.ToList();
            //sort by passed identifiers
            var sortedCustomers = new List<Customer>();
            foreach (int id in customerIds)
            {
                var customer = customers.Find(x => x.Id == id);
                if (customer != null)
                    sortedCustomers.Add(customer);
            }
            return sortedCustomers;
        }

		public virtual IList<Customer> GetSystemAccountCustomers()
		{
			return _customerRepository.Table.Where(x => x.IsSystemAccount).ToList();
		}

        public virtual Customer GetCustomerByGuid(Guid customerGuid)
        {
            if (customerGuid == Guid.Empty)
                return null;

            var query = from c in IncludeShoppingCart(_customerRepository.Table)
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

            var query = from c in IncludeShoppingCart(_customerRepository.Table)
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

            var query = from c in IncludeShoppingCart(_customerRepository.Table)
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
                throw new SmartException("'Guests' role could not be loaded");

            customer.CustomerRoles.Add(guestRole);
            _customerRepository.Insert(customer);

			var clientIdent = _services.WebHelper.GetClientIdent();
			if (clientIdent.HasValue())
			{
				_genericAttributeService.SaveAttribute(customer, "ClientIdent", clientIdent);
			}

			Logger.DebugFormat("Guest account created for anonymous visitor. Id: {0}, ClientIdent: {1}", customer.CustomerGuid, clientIdent ?? "n/a");

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

				var query = from a in _gaRepository.TableUntracked
							join c in _customerRepository.Table on a.EntityId equals c.Id into Customers
							from c in Customers.DefaultIfEmpty()
							where c.LastActivityDateUtc >= dateFrom
								&& c.Username == null
								&& c.Email == null
								&& a.KeyGroup == "Customer"
								&& a.Key == "ClientIdent"
								&& a.Value == clientIdent
							select c;

				return query.FirstOrDefault();
			}
		}

		public virtual void InsertCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            _customerRepository.Insert(customer);

            _services.EventPublisher.EntityInserted(customer);
        }
        
        public virtual void UpdateCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            _customerRepository.Update(customer);

			_services.EventPublisher.EntityUpdated(customer);
        }

		public virtual void ResetCheckoutData(Customer customer, int storeId,
            bool clearCouponCodes = false, bool clearCheckoutAttributes = false,
            bool clearRewardPoints = true, bool clearShippingMethod = true,
            bool clearPaymentMethod = true)
        {
            if (customer == null)
                throw new ArgumentNullException();

            //clear entered coupon codes
            if (clearCouponCodes)
            {
				_genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.DiscountCouponCode, null);
				_genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.GiftCardCouponCodes, null);
            }

            //clear checkout attributes
            if (clearCheckoutAttributes)
            {
				_genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.CheckoutAttributes, null);
            }

            //clear reward points flag
            if (clearRewardPoints)
            {
				_genericAttributeService.SaveAttribute<bool>(customer, SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, false, storeId);
            }

            //clear selected shipping method
            if (clearShippingMethod)
            {
				_genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.SelectedShippingOption, null, storeId);
				_genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.OfferedShippingOptions, null, storeId);
            }

            //clear selected payment method
            if (clearPaymentMethod)
            {
				_genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, null, storeId);
            }

            UpdateCustomer(customer);
        }
        
        public virtual int DeleteGuestCustomers(DateTime? registrationFrom, DateTime? registrationTo, bool onlyWithoutShoppingCart, int maxItemsToDelete = 5000)
        {
			var ctx = _customerRepository.Context;

			using (var scope = new DbContextScope(ctx: ctx, autoDetectChanges: false, proxyCreation: true, validateOnSave: false, forceNoTracking: true, autoCommit: false))
			{
				var guestRole = GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests);
				if (guestRole == null)
					throw new SmartException("'Guests' role could not be loaded");
				
				var query = _customerRepository.Table;

				if (registrationFrom.HasValue)
					query = query.Where(c => registrationFrom.Value <= c.CreatedOnUtc);
				if (registrationTo.HasValue)
					query = query.Where(c => registrationTo.Value >= c.CreatedOnUtc);

				query = query.Where(c => c.CustomerRoles.Select(cr => cr.Id).Contains(guestRole.Id));

				if (onlyWithoutShoppingCart)
					query = query.Where(c => !c.ShoppingCartItems.Any());

				// no orders
				query = JoinWith<Order>(query, x => x.CustomerId);

				// no customer content
				query = JoinWith<CustomerContent>(query, x => x.CustomerId);

				// no private messages (guests can only receive but not send messages)
				query = JoinWith<PrivateMessage>(query, x => x.ToCustomerId);

				// no forum posts
				query = JoinWith<ForumPost>(query, x => x.CustomerId);

				// no forum topics
				query = JoinWith<ForumTopic>(query, x => x.CustomerId);

				//don't delete system accounts
				query = query.Where(c => !c.IsSystemAccount);

				// only distinct items
				query = from c in query
						group c by c.Id
							into cGroup
							orderby cGroup.Key
							select cGroup.FirstOrDefault();
				query = query.OrderBy(c => c.Id);

				var customers = query.Take(maxItemsToDelete).ToList();

				int numberOfDeletedCustomers = 0;
				foreach (var c in customers)
				{
					try
					{
						// delete attributes (using GenericAttributeService would incorporate caching... which is bad in long running processes)
						var gaQuery = from ga in _gaRepository.Table
									  where ga.EntityId == c.Id &&
									  ga.KeyGroup == "Customer"
									  select ga;
						var attributes = gaQuery.ToList();

						_gaRepository.DeleteRange(attributes);
						
						// delete customer
						_customerRepository.Delete(c);
						numberOfDeletedCustomers++;

						if (numberOfDeletedCustomers % 1000 == 0)
						{
							// save changes all 1000th item
							try
							{
								scope.Commit();
							}
							catch (Exception ex) 
							{
								Debug.WriteLine(ex);
							}
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex);
					}
				}

				// save the rest
				scope.Commit();

				return numberOfDeletedCustomers;
			}
        }

		private IQueryable<Customer> JoinWith<T>(IQueryable<Customer> query, Expression<Func<T, int>> customerIdSelector) where T : BaseEntity
		{
			var inner = _customerRepository.Context.Set<T>().AsNoTracking();

			/* 
			 * Lamda join created with LinqPad. ORIGINAL:
				 from c in customers
					join inner in ctx.Set<TInner>().AsNoTracking() on c.Id equals inner.CustomerId into c_inner
					from inner in c_inner.DefaultIfEmpty()
					where !c_inner.Any()
					select c;
			*/
			query = query
				.GroupJoin(
					inner,
					c => c.Id,
					customerIdSelector,
					(c, i) => new { Customer = c, Inner = i })
				.SelectMany(
					x => x.Inner.DefaultIfEmpty(),
					(a, b) => new { a, b }
				)
				.Where(x => !(x.a.Inner.Any()))
				.Select(x => x.a.Customer);

			return query;
		}

        #endregion
        
        #region Customer roles

        public virtual void DeleteCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("customerRole");

            if (customerRole.IsSystemRole)
                throw new SmartException("System role could not be deleted");

            _customerRoleRepository.Delete(customerRole);

			_services.EventPublisher.EntityDeleted(customerRole);
        }

        public virtual CustomerRole GetCustomerRoleById(int customerRoleId)
        {
            if (customerRoleId == 0)
                return null;

            return _customerRoleRepository.GetById(customerRoleId);
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

        public virtual IList<CustomerRole> GetAllCustomerRoles(bool showHidden = false)
        {
			var query = from cr in _customerRoleRepository.Table
						orderby cr.Name
						where (showHidden || cr.Active)
						select cr;

			var customerRoles = query.ToListCached();
			return customerRoles;
		}
        
        public virtual void InsertCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("customerRole");

            _customerRoleRepository.Insert(customerRole);

			_services.EventPublisher.EntityInserted(customerRole);
        }

        public virtual void UpdateCustomerRole(CustomerRole customerRole)
        {
            if (customerRole == null)
                throw new ArgumentNullException("customerRole");

            _customerRoleRepository.Update(customerRole);

			_services.EventPublisher.EntityUpdated(customerRole);
        }

        #endregion

		#region Reward points

		public virtual void RewardPointsForProductReview(Customer customer, Product product, bool add)
		{
			if (_rewardPointsSettings.Enabled && _rewardPointsSettings.PointsForProductReview > 0)
			{
				string message = T(add ? "RewardPoints.Message.EarnedForProductReview" : "RewardPoints.Message.ReducedForProductReview", product.GetLocalized(x => x.Name));

				customer.AddRewardPointsHistoryEntry(_rewardPointsSettings.PointsForProductReview * (add ? 1 : -1), message);

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

		#endregion Reward points
	}
}