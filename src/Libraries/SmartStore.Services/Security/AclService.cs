using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Services.Customers;

namespace SmartStore.Services.Security
{
    public partial class AclService : IAclService
    {
		/// <summary>
		/// 0 = segment (EntityName.IdRange)
		/// </summary>
		const string ACL_SEGMENT_KEY = "acl:range-{0}";
		const string ACL_SEGMENT_PATTERN = "acl:range-*";

        private readonly IRepository<AclRecord> _aclRecordRepository;
        private readonly Work<IWorkContext> _workContext;
        private readonly ICacheManager _cacheManager;
		private readonly ICustomerService _customerService;

		private bool? _hasActiveAcl;

        public AclService(
			ICacheManager cacheManager, 
			Work<IWorkContext> workContext, 
			IRepository<AclRecord> aclRecordRepository,
			ICustomerService customerService)
        {
            _cacheManager = cacheManager;
            _workContext = workContext;
            _aclRecordRepository = aclRecordRepository;
			_customerService = customerService;
		}

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

			ClearCacheSegment(aclRecord.EntityName, aclRecord.EntityId);
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
            string entityName = entity.GetEntityName();

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

		public virtual void SaveAclMappings<T>(T entity, int[] selectedCustomerRoleIds) where T : BaseEntity, IAclSupported
		{
			var existingAclRecords = GetAclRecords(entity);
			var allCustomerRoles = _customerService.GetAllCustomerRoles(true);

			foreach (var customerRole in allCustomerRoles)
			{
				if (selectedCustomerRoleIds != null && selectedCustomerRoleIds.Contains(customerRole.Id))
				{
					// New role
					if (!existingAclRecords.Any(x => x.CustomerRoleId == customerRole.Id))
						InsertAclRecord(entity, customerRole.Id);
				}
				else
				{
					// Removed role
					var aclRecordToDelete = existingAclRecords.FirstOrDefault(x => x.CustomerRoleId == customerRole.Id);
					if (aclRecordToDelete != null)
						DeleteAclRecord(aclRecordToDelete);
				}
			}
		}

		public virtual void InsertAclRecord(AclRecord aclRecord)
        {
			Guard.NotNull(aclRecord, nameof(aclRecord));

			_aclRecordRepository.Insert(aclRecord);

			ClearCacheSegment(aclRecord.EntityName, aclRecord.EntityId);
		}

        public virtual void InsertAclRecord<T>(T entity, int customerRoleId) where T : BaseEntity, IAclSupported
        {
			Guard.NotNull(entity, nameof(entity));
			
			if (customerRoleId == 0)
                throw new ArgumentOutOfRangeException(nameof(customerRoleId));

            int entityId = entity.Id;
            string entityName = entity.GetEntityName();

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

			ClearCacheSegment(aclRecord.EntityName, aclRecord.EntityId);
		}

		public virtual int[] GetCustomerRoleIdsWithAccess(string entityName, int entityId)
		{
			Guard.NotEmpty(entityName, nameof(entityName));

			if (entityId <= 0)
				return new int[0];

			var cacheSegment = GetCacheSegment(entityName, entityId);

			if (!cacheSegment.TryGetValue(entityId, out var roleIds))
			{
				return Array.Empty<int>();
			}

			return roleIds;
		}

		public bool Authorize(string entityName, int entityId)
		{
			return Authorize(entityName, entityId, _workContext.Value.CurrentCustomer);
		}

		public virtual bool Authorize(string entityName, int entityId, Customer customer)
		{
			Guard.NotEmpty(entityName, nameof(entityName));

			if (entityId <= 0)
				return false;

			if (customer == null)
				return false;

			if (!HasActiveAcl)
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

		#region Cache segmenting

		protected virtual IDictionary<int, int[]> GetCacheSegment(string entityName, int entityId)
		{
			Guard.NotEmpty(entityName, nameof(entityName));

			var segmentKey = GetSegmentKeyPart(entityName, entityId, out var minEntityId, out var maxEntityId);
			var cacheKey = BuildCacheSegmentKey(segmentKey);

			return _cacheManager.Get(cacheKey, () =>
			{
				var query = from sm in _aclRecordRepository.TableUntracked
							where
								sm.EntityId >= minEntityId &&
								sm.EntityId <= maxEntityId &&
								sm.EntityName == entityName
							select sm;

				var mappings = query.ToLookup(x => x.EntityId, x => x.CustomerRoleId);

				var dict = new Dictionary<int, int[]>(mappings.Count);

				foreach (var sm in mappings)
				{
					dict[sm.Key] = sm.ToArray();
				}

				return dict;
			});
		}

		/// <summary>
		/// Clears the cached segment from the cache
		/// </summary>
		protected virtual void ClearCacheSegment(string entityName, int entityId)
		{
			try
			{
				var segmentKey = GetSegmentKeyPart(entityName, entityId);
				_cacheManager.Remove(BuildCacheSegmentKey(segmentKey));
			}
			catch { }
		}

		private string BuildCacheSegmentKey(string segment)
		{
			return String.Format(ACL_SEGMENT_KEY, segment);
		}

		private string GetSegmentKeyPart(string entityName, int entityId)
		{
			return GetSegmentKeyPart(entityName, entityId, out _, out _);
		}

		private string GetSegmentKeyPart(string entityName, int entityId, out int minId, out int maxId)
		{
			maxId = entityId.GetRange(1000, out minId);
			return (entityName + "." + maxId.ToString()).ToLowerInvariant();
		}

		#endregion
	}
}