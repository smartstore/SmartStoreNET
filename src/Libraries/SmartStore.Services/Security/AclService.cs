using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Services.Security
{
    public partial class AclService : IAclService
    {
        private const string ACLRECORD_BY_ENTITYID_NAME_KEY = "aclrecord:entityid-name-{0}-{1}";
        private const string ACLRECORD_PATTERN_KEY = "aclrecord:*";


        private readonly IRepository<AclRecord> _aclRecordRepository;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
		private bool? _hasActiveAcl;

        public AclService(ICacheManager cacheManager, IWorkContext workContext,
            IRepository<AclRecord> aclRecordRepository)
        {
            _cacheManager = cacheManager;
            _workContext = workContext;
            _aclRecordRepository = aclRecordRepository;

			QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

		public bool HasActiveAcl 
		{
			get
			{
				if (!_hasActiveAcl.HasValue)
				{
					_hasActiveAcl = _aclRecordRepository.TableUntracked.Any(x => !x.IsIdle);
				}
				return _hasActiveAcl.Value;
			}
		}

        public virtual void DeleteAclRecord(AclRecord aclRecord)
        {
			Guard.NotNull(aclRecord,nameof(aclRecord));

            _aclRecordRepository.Delete(aclRecord);

            _cacheManager.RemoveByPattern(ACLRECORD_PATTERN_KEY);
        }

        public virtual AclRecord GetAclRecordById(int aclRecordId)
        {
            if (aclRecordId == 0)
                return null;

            var aclRecord = _aclRecordRepository.GetById(aclRecordId);
            return aclRecord;
        }

        public IList<AclRecord> GetAclRecords<T>(T entity) where T : BaseEntity, IAclSupported
        {
			Guard.NotNull(entity, nameof(entity));

			int entityId = entity.Id;
            string entityName = typeof(T).Name;

			return GetAclRecordsFor(entityName, entityId);
        }

		public virtual IList<AclRecord> GetAclRecordsFor(string entityName, int entityId)
		{
			Guard.IsPositive(entityId, nameof(entityId));
			Guard.NotEmpty(entityName, nameof(entityName));

			var query = from ur in _aclRecordRepository.Table
						where ur.EntityId == entityId &&
						ur.EntityName == entityName
						select ur;
			var aclRecords = query.ToList();
			return aclRecords;
		}


        public virtual void InsertAclRecord(AclRecord aclRecord)
        {
			Guard.NotNull(aclRecord, nameof(aclRecord));

			_aclRecordRepository.Insert(aclRecord);

            _cacheManager.RemoveByPattern(ACLRECORD_PATTERN_KEY);
        }

        public virtual void InsertAclRecord<T>(T entity, int customerRoleId) where T : BaseEntity, IAclSupported
        {
			Guard.NotNull(entity, nameof(entity));
			
			if (customerRoleId == 0)
                throw new ArgumentOutOfRangeException(nameof(customerRoleId));

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var aclRecord = new AclRecord
            {
                EntityId = entityId,
                EntityName = entityName,
                CustomerRoleId = customerRoleId
            };

            InsertAclRecord(aclRecord);
        }

        public virtual void UpdateAclRecord(AclRecord aclRecord)
        {
			Guard.NotNull(aclRecord, nameof(aclRecord));

			_aclRecordRepository.Update(aclRecord);

            _cacheManager.RemoveByPattern(ACLRECORD_PATTERN_KEY);
        }

		public virtual int[] GetCustomerRoleIdsWithAccess(string entityName, int entityId)
		{
			Guard.NotEmpty(entityName, nameof(entityName));

			if (entityId <= 0)
				return new int[0];

			string key = string.Format(ACLRECORD_BY_ENTITYID_NAME_KEY, entityId, entityName);
			return _cacheManager.Get(key, () =>
			{
				var query = from ur in _aclRecordRepository.Table
							where ur.EntityId == entityId &&
							ur.EntityName == entityName
							select ur.CustomerRoleId;

				var result = query.ToArray();
				return result;
			});
		}

		public bool Authorize(string entityName, int entityId)
		{
			return Authorize(entityName, entityId, _workContext.CurrentCustomer);
		}

		public virtual bool Authorize(string entityName, int entityId, Customer customer)
		{
			Guard.NotEmpty(entityName, nameof(entityName));

			if (entityId <= 0)
				return false;

			if (customer == null)
				return false;

			if (QuerySettings.IgnoreAcl)
				return true;

			foreach (var role1 in customer.CustomerRoles.Where(cr => cr.Active))
			{
				foreach (var role2Id in GetCustomerRoleIdsWithAccess(entityName, entityId))
				{
					if (role1.Id == role2Id)
					{
						// yes, we have such permission
						return true;
					}
				}
			}

			// no permission granted
			return false;
		}
	}
}