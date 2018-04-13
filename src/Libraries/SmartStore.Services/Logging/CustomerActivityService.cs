using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Logging;

namespace SmartStore.Services.Logging
{
	/// <summary>
	/// Customer activity service
	/// </summary>
	public class CustomerActivityService : ICustomerActivityService
    {
		#region Fields

		private const int _deleteNumberOfEntries = 1000;

		private readonly IRepository<ActivityLog> _activityLogRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
		private readonly IRepository<Customer> _customerRepository;
		private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;

        private readonly static object s_lock = new object();
        private readonly static ConcurrentDictionary<string, ActivityLogType> s_logTypes = new ConcurrentDictionary<string, ActivityLogType>();

		#endregion

		#region Ctor

		public CustomerActivityService(
            IRepository<ActivityLog> activityLogRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
			IRepository<Customer> customerRepository,
			IWorkContext workContext,
            IDbContext dbContext)
        {
            this._activityLogRepository = activityLogRepository;
            this._activityLogTypeRepository = activityLogTypeRepository;
			this._customerRepository = customerRepository;
            this._workContext = workContext;
            this._dbContext = dbContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type item</param>
        public virtual void InsertActivityType(ActivityLogType activityLogType)
        {
            if (activityLogType == null)
                throw new ArgumentNullException("activityLogType");

            s_logTypes.Clear();
            _activityLogTypeRepository.Insert(activityLogType);
        }

        /// <summary>
        /// Updates an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type item</param>
        public virtual void UpdateActivityType(ActivityLogType activityLogType)
        {
            if (activityLogType == null)
                throw new ArgumentNullException("activityLogType");

            s_logTypes.Clear();
            _activityLogTypeRepository.Update(activityLogType);
        }

        /// <summary>
        /// Deletes an activity log type item
        /// </summary>
        /// <param name="activityLogType">Activity log type</param>
        public virtual void DeleteActivityType(ActivityLogType activityLogType)
        {
            if (activityLogType == null)
                throw new ArgumentNullException("activityLogType");

            s_logTypes.Clear();
            _activityLogTypeRepository.Delete(activityLogType);
        }

        /// <summary>
        /// Gets all activity log type items
        /// </summary>
        /// <returns>Activity log type collection</returns>
        public virtual IEnumerable<ActivityLogType> GetAllActivityTypes()
        {
            EnsureLogTypesAreLoaded();
            return s_logTypes.Select(x => x.Value);
        }

        private void EnsureLogTypesAreLoaded()
        {
            if (s_logTypes.Count > 0)
                return;

            lock (s_lock)
            {
                if (s_logTypes.Count == 0)
                {
                    var query = from alt in _activityLogTypeRepository.Table
                                orderby alt.Name
                                select alt;
                    query.Each(x => {
                        s_logTypes[x.SystemKeyword] = x;
                    });
                }
            }
        }

        /// <summary>
        /// Gets an activity log type item
        /// </summary>
        /// <param name="activityLogTypeId">Activity log type identifier</param>
        /// <returns>Activity log type item</returns>
        public virtual ActivityLogType GetActivityTypeById(int activityLogTypeId)
        {
            if (activityLogTypeId == 0)
                return null;

            return _activityLogTypeRepository.GetById(activityLogTypeId);
        }

        public virtual ActivityLogType GetActivityTypeBySystemKeyword(string systemKeyword)
        {
            if (systemKeyword.IsEmpty())
                return null;

            EnsureLogTypesAreLoaded();

            if (s_logTypes.ContainsKey(systemKeyword))
            {
                return s_logTypes[systemKeyword];
            }

            return null;
        }

        /// <summary>
        /// Inserts an activity log item
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        public virtual ActivityLog InsertActivity(string systemKeyword, string comment, params object[] commentParams)
        {
            return InsertActivity(systemKeyword, comment, _workContext.CurrentCustomer, commentParams);
        }


        /// <summary>
        /// Inserts an activity log item
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <param name="comment">The activity comment</param>
        /// <param name="customer">The customer</param>
        /// <param name="commentParams">The activity comment parameters for string.Format() function.</param>
        /// <returns>Activity log item</returns>
        public virtual ActivityLog InsertActivity(string systemKeyword, string comment, Customer customer, params object[] commentParams)
        {
            if (customer == null)
                return null;

            var activityType = this.GetActivityTypeBySystemKeyword(systemKeyword);
            if (activityType == null || !activityType.Enabled)
                return null;

			comment = comment.EmptyNull();
            comment = string.Format(comment, commentParams);
			comment = comment.Truncate(4000);

            var activity = new ActivityLog();
            activity.ActivityLogTypeId = activityType.Id;
            activity.CustomerId = customer.Id;
            activity.Comment = comment;
            activity.CreatedOnUtc = DateTime.UtcNow;

            _activityLogRepository.Insert(activity);

            return activity;
        }

        /// <summary>
        /// Deletes an activity log item
        /// </summary>
        /// <param name="activityLog">Activity log type</param>
        public virtual void DeleteActivity(ActivityLog activityLog)
        {
            if (activityLog == null)
                throw new ArgumentNullException("activityLog");

            _activityLogRepository.Delete(activityLog);
        }

		/// <summary>
		/// Gets all activity log items
		/// </summary>
		/// <param name="createdOnFrom">Log item creation from; null to load all customers</param>
		/// <param name="createdOnTo">Log item creation to; null to load all customers</param>
		/// <param name="customerId">Customer identifier; null to load all customers</param>
		/// <param name="activityLogTypeId">Activity log type identifier</param>
		/// <param name="pageIndex">Page index</param>
		/// <param name="pageSize">Page size</param>
		/// <param name="email">Customer email</param>
		/// <param name="customerSystemAccount">Customer system name</param>
		/// <returns>Activity log collection</returns>
		public virtual IPagedList<ActivityLog> GetAllActivities(
			DateTime? createdOnFrom,
            DateTime? createdOnTo,
			int? customerId,
			int activityLogTypeId,
            int pageIndex,
			int pageSize,
			string email = null,
			bool? customerSystemAccount = null)
        {
            var query = _activityLogRepository.Table;

			if (email.HasValue() || customerSystemAccount.HasValue)
			{
				var queryCustomers = _customerRepository.Table;

				if (email.HasValue())
					queryCustomers = queryCustomers.Where(x => x.Email.Contains(email));

				if (customerSystemAccount.HasValue)
					queryCustomers = queryCustomers.Where(x => x.IsSystemAccount == customerSystemAccount.Value);

				query =
					from al in _activityLogRepository.Table
					join c in queryCustomers on al.CustomerId equals c.Id
					select al;
			}

            if (createdOnFrom.HasValue)
                query = query.Where(al => createdOnFrom.Value <= al.CreatedOnUtc);

            if (createdOnTo.HasValue)
                query = query.Where(al => createdOnTo.Value >= al.CreatedOnUtc);

            if (activityLogTypeId > 0)
                query = query.Where(al => activityLogTypeId == al.ActivityLogTypeId);

            if (customerId.HasValue)
                query = query.Where(al => customerId.Value == al.CustomerId);

            query = query.OrderByDescending(al => al.CreatedOnUtc);

            var activityLog = new PagedList<ActivityLog>(query, pageIndex, pageSize);
            return activityLog;
        }

        /// <summary>
        /// Gets an activity log item
        /// </summary>
        /// <param name="activityLogId">Activity log identifier</param>
        /// <returns>Activity log item</returns>
        public virtual ActivityLog GetActivityById(int activityLogId)
        {
            if (activityLogId == 0)
                return null;


            var query = from al in _activityLogRepository.Table
                        where al.Id == activityLogId
                        select al;
            var activityLog = query.SingleOrDefault();
            return activityLog;
        }

		public virtual IList<ActivityLog> GetActivityByIds(int[] activityLogIds)
		{
			if (activityLogIds == null || activityLogIds.Length == 0)
				return new List<ActivityLog>();

			var query = _activityLogRepository.Table
				.Where(x => activityLogIds.Contains(x.Id))
				.OrderByDescending(x => x.CreatedOnUtc);

			return query.ToList();
		}

		/// <summary>
		/// Clears activity log
		/// </summary>
		public virtual void ClearAllActivities()
        {
			try
			{
				_dbContext.ExecuteSqlCommand("TRUNCATE TABLE [ActivityLog]");
			}
			catch
			{
				try
				{
					for (int i = 0; i < 100000; ++i)
					{
						if (_dbContext.ExecuteSqlCommand("Delete Top ({0}) From [ActivityLog]", false, null, _deleteNumberOfEntries) < _deleteNumberOfEntries)
							break;
					}
				}
				catch { }

				try
				{
					_dbContext.ExecuteSqlCommand("DBCC CHECKIDENT('ActivityLog', RESEED, 0)");
				}
				catch
				{
					try
					{
						_dbContext.ExecuteSqlCommand("Alter Table [ActivityLog] Alter Column [Id] Identity(1,1)");
					}
					catch { }
				}
			}
		}
       
		#endregion
    }
}
