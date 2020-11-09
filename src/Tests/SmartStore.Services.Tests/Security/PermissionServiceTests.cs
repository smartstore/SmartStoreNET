using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Fakes;
using SmartStore.Core.Security;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;

namespace SmartStore.Services.Tests.Security
{
    [TestFixture]
    public class PermissionServiceTests
    {
        private IPermissionService _permissionService;
        private IRepository<PermissionRecord> _permissionRepository;
        private IRepository<PermissionRoleMapping> _permissionMappingRepository;
        private Lazy<ICustomerService> _customerService;
        private ILocalizationService _localizationService;
        private IWorkContext _workContext;
        private ICacheManager _cacheManager;
        private IRequestCache _requestCache;

        private CustomerRole _rAdmin = new CustomerRole { Id = 1, Active = true, SystemName = "Administrators" };
        private CustomerRole _rModerator = new CustomerRole { Id = 2, Active = true, SystemName = "Moderators" };
        private CustomerRole _rGuest = new CustomerRole { Id = 3, Active = true, SystemName = "Guests" };

        private Customer _cAdmin = new Customer { Id = 1, Username = "Admin" };
        private Customer _cModerator = new Customer { Id = 2, Username = "Moderator" };
        private Customer _cGuest = new Customer { Id = 3, Username = "Guest" };

        [SetUp]
        public virtual void Setup()
        {
            _permissionRepository = MockRepository.GenerateMock<IRepository<PermissionRecord>>();
            _permissionMappingRepository = MockRepository.GenerateMock<IRepository<PermissionRoleMapping>>();
            _customerService = MockRepository.GenerateMock<Lazy<ICustomerService>>();
            _localizationService = MockRepository.GenerateMock<ILocalizationService>();
            _workContext = MockRepository.GenerateMock<IWorkContext>();
            _cacheManager = NullCache.Instance;
            _requestCache = new RequestCache(new FakeHttpContext("~/"));

            _permissionService = new PermissionService(
                _permissionRepository,
                _permissionMappingRepository,
                _customerService,
                _localizationService,
                _workContext,
                _cacheManager,
                _requestCache);

            _cAdmin.CustomerRoleMappings.Add(new CustomerRoleMapping
            {
                CustomerId = _cAdmin.Id,
                CustomerRoleId = _rAdmin.Id,
                CustomerRole = _rAdmin
            });

            _cModerator.CustomerRoleMappings.Add(new CustomerRoleMapping
            {
                CustomerId = _cModerator.Id,
                CustomerRoleId = _rGuest.Id,
                CustomerRole = _rGuest
            });
            _cModerator.CustomerRoleMappings.Add(new CustomerRoleMapping
            {
                CustomerId = _cModerator.Id,
                CustomerRoleId = _rModerator.Id,
                CustomerRole = _rModerator
            });

            _cGuest.CustomerRoleMappings.Add(new CustomerRoleMapping
            {
                CustomerId = _cGuest.Id,
                CustomerRoleId = _rGuest.Id,
                CustomerRole = _rGuest
            });

            var pCatalog = new PermissionRecord { Id = 1, SystemName = "catalog" };

            var pManu = new PermissionRecord { Id = 10, SystemName = "catalog.manufacturer" };
            var pManuRead = new PermissionRecord { Id = 100, SystemName = "catalog.manufacturer.read" };
            var pManuWrite = new PermissionRecord { Id = 101, SystemName = "catalog.manufacturer.write" };

            var pCategory = new PermissionRecord { Id = 20, SystemName = "catalog.category" };
            var pCategoryRead = new PermissionRecord { Id = 200, SystemName = "catalog.category.read" };
            var pCategoryWrite = new PermissionRecord { Id = 201, SystemName = "catalog.category.write" };

            AddMapping(pManu, _rAdmin, true);
            AddMapping(pCategory, _rAdmin, true);

            AddMapping(pManu, _rModerator, false);
            AddMapping(pManuRead, _rModerator, true);
            AddMapping(pManuWrite, _rModerator, false);

            AddMapping(pCategory, _rGuest, false);

            var permissions = new List<PermissionRecord> { pCatalog, pManu, pManuRead, pManuWrite, pCategory, pCategoryRead, pCategoryWrite };

            _permissionRepository.Expect(x => x.Table).Return(permissions.AsQueryable());
            _permissionRepository.Expect(x => x.TableUntracked).Return(permissions.AsQueryable());
        }

        [Test]
        public void Permission_allow()
        {
            CheckTreeNode(_rModerator, "catalog.manufacturer.read", true);

            var result = _permissionService.Authorize("catalog.manufacturer", _cModerator);
            Assert.IsFalse(result);

            result = _permissionService.Authorize("catalog.manufacturer.read", _cModerator);
            Assert.IsTrue(result);
        }

        [Test]
        public void Permission_deny()
        {
            CheckTreeNode(_rModerator, "catalog.manufacturer.write", false);

            var result = _permissionService.Authorize("catalog.manufacturer.write", _cModerator);
            Assert.IsFalse(result);
        }

        [Test]
        public void Permission_allow_by_parent()
        {
            CheckTreeNode(_rAdmin, "catalog.category", true);

            var result = _permissionService.Authorize("catalog.category.write", _cAdmin);
            Assert.IsTrue(result);
        }

        [Test]
        public void Permission_deny_by_parent()
        {
            CheckTreeNode(_rGuest, "catalog.category", false);

            var result = _permissionService.Authorize("catalog.category.read", _cGuest);
            Assert.IsFalse(result);

            result = _permissionService.Authorize("catalog.manufacturer.write", _cGuest);
            Assert.IsFalse(result);
        }

        [Test]
        public void Permission_find()
        {
            CheckTreeNode(_rModerator, "catalog.manufacturer", false);

            var result = _permissionService.FindAuthorization("catalog.manufacturer", _cModerator);
            Assert.IsTrue(result);

            result = _permissionService.FindAuthorization("catalog.product", _cModerator);
            Assert.IsFalse(result);
        }

        private void AddMapping(PermissionRecord permission, CustomerRole role, bool allow)
        {
            permission.PermissionRoleMappings.Add(new PermissionRoleMapping
            {
                CustomerRoleId = role.Id,
                CustomerRole = role,
                PermissionRecordId = permission.Id,
                PermissionRecord = permission,
                Allow = allow
            });
        }

        private void CheckTreeNode(CustomerRole role, string permissionSystemName, bool allow)
        {
            var tree = _permissionService.GetPermissionTree(role);
            var node = tree.SelectNodeById(permissionSystemName);

            Assert.NotNull(node, $"Cannot select node by id '{permissionSystemName}'.");
            Assert.NotNull(node.Value.Allow, "The selected node must not be 'null'.");

            if (allow)
            {
                Assert.IsTrue(node.Value.Allow.Value, "The value of the selected node must be 'true'.");
            }
            else
            {
                Assert.IsFalse(node.Value.Allow.Value, "The value of the selected node must be 'false'.");
            }
        }
    }
}
