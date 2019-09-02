using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Security
{
    public partial class PermissionService2 : IPermissionService2
    {
        // {0} = roleId
        internal const string PERMISSION_TREE_KEY = "permission:tree-{0}";
        private const string PERMISSION_TREE_PATTERN_KEY = "permission:tree-*";

        private static readonly Dictionary<string, string> _displayNameResourceKeys = new Dictionary<string, string>
        {
            { "read", "Common.Read" },
            { "update", "Common.Edit" },
            { "create", "Common.Create" },
            { "delete", "Common.Delete" },
            { "catalog", "Admin.Catalog" },
            { "product", "Admin.Catalog.Products" },
            { "category", "Admin.Catalog.Categories" },
            { "manufacturer", "Admin.Catalog.Manufacturers" },
            { "variant", "Admin.Catalog.Attributes.ProductAttributes" },
            { "attribute", "Admin.Catalog.Attributes.SpecificationAttributes" },
            { "customer", "Admin.Customers" },
            { "impersonate", "Admin.Customers.Customers.Impersonate" },
            { "role", "Admin.Customers.CustomerRoles" },
            { "order", "Admin.Orders" },
            { "gift-card", "Admin.GiftCards" },
            { "notify", "Common.Notify" },
            { "return-request", "Admin.ReturnRequests" },
            { "accept", "Admin.ReturnRequests.Accept" },
            { "promotion", "Admin.Catalog.Products.Promotion" },
            { "affiliate", "Admin.Affiliates" },
            { "campaign", "Admin.Promotions.Campaigns" },
            { "discount", "Admin.Promotions.Discounts" },
            { "newsletter", "Admin.Promotions.NewsLetterSubscriptions" },
            { "cms", "Admin.ContentManagement" },
            { "poll", "Admin.ContentManagement.Polls" },
            { "news", "Admin.ContentManagement.News" },
            { "blog", "Admin.ContentManagement.Blog" },
            { "widget", "Admin.ContentManagement.Widgets" },
            { "topic", "Admin.ContentManagement.Topics" },
            { "menu", "Admin.ContentManagement.Menus" },
            { "forum", "Admin.ContentManagement.Forums" },
            { "message-template", "Admin.ContentManagement.MessageTemplates" },
            { "configuration", "Admin.Configuration" },
            { "country", "Admin.Configuration.Countries" },
            { "language", "Admin.Configuration.Languages" },
            { "setting", "Admin.Configuration.Settings" },
            { "payment-method", "Admin.Configuration.Payment.Methods" },
            { "activate", "Admin.Common.Activate" },
            { "authentication", "Admin.Configuration.ExternalAuthenticationMethods" },
            { "currency", "Admin.Configuration.Currencies" },
            { "delivery-time", "Admin.Configuration.DeliveryTimes" },
            { "theme", "Admin.Configuration.Themes" },
            { "measure", "Admin.Configuration.Measures.Dimensions" },
            { "activity-log", "Admin.Configuration.ActivityLog.ActivityLogType" },
            { "acl", "Admin.Configuration.ACL" },
            { "email-account", "Admin.Configuration.EmailAccounts" },
            { "store", "Admin.Common.Stores" },
            { "shipping", "Admin.Configuration.DeliveryTimes" },
            { "tax", "Admin.Configuration.Tax.Providers" },
            { "plugin", "Admin.Configuration.Plugins" },
            { "upload", "Common.Upload" },
            { "install", "Admin.Configuration.Plugins.Fields.Install" },
            { "license", "Admin.Common.License" },
            { "export", "Common.Export" },
            { "execute", "Admin.Common.Go" },
            { "import", "Common.Import" },
            { "system", "Admin.System" },
            { "administrate", "Admin.Plugins.KnownGroup.Admin" },
            { "log", "Admin.System.Log" },
            { "message", "Admin.System.QueuedEmails" },
            { "send", "Common.Send" },
            { "maintenance", "Admin.System.Maintenance" },
            { "schedule-task", "Admin.System.ScheduleTasks" },
            { "url-record", "Admin.System.SeNames" },
            { "cart", "ShoppingCart" },
            { "checkout-attribute", "Admin.Catalog.Attributes.CheckoutAttributes" },
            { "media", "Admin.Plugins.KnownGroup.Media" }
        };

        private readonly IRepository<PermissionRecord> _permissionRepository;
        private readonly IRepository<PermissionRoleMapping> _permissionMappingRepository;
        private readonly Lazy<ICustomerService> _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;

        public PermissionService2(
            IRepository<PermissionRecord> permissionRepository,
            IRepository<PermissionRoleMapping> permissionMappingRepository,
            Lazy<ICustomerService> customerService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICacheManager cacheManager)
        {
            _permissionRepository = permissionRepository;
            _permissionMappingRepository = permissionMappingRepository;
            _customerService = customerService;
            _localizationService = localizationService;
            _workContext = workContext;
            _cacheManager = cacheManager;
        }

        public virtual PermissionRecord GetPermissionById(int permissionId)
        {
            if (permissionId == 0)
            {
                return null;
            }

            return _permissionRepository.GetById(permissionId);
        }

        public virtual PermissionRecord GetPermissionBySystemName(string systemName)
        {
            if (systemName.IsEmpty())
            {
                return null;
            }

            var permission = _permissionRepository.Table
                .Where(x => x.SystemName == systemName)
                .OrderBy(x => x.Id)
                .FirstOrDefault();

            return permission;
        }

        public virtual IList<PermissionRecord> GetAllPermissions()
        {
            var permissions = _permissionRepository.Table.ToList();
            return permissions;
        }

        public virtual IDictionary<string, string> GetAllSystemNames()
        {
            var language = _workContext.WorkingLanguage;
            var resourcesLookup = GetDisplayNameLookup(language.Id);

            Func<string, string> nameSelector = x =>
            {
                var tokens = x.EmptyNull().ToLower().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (!tokens.Any())
                {
                    return string.Empty;
                }

                var sb = new StringBuilder();

                foreach (var token in tokens)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(" » ");
                    }

                    var displayName = GetDisplayName(token, language.Id, resourcesLookup) ?? token ?? string.Empty;
                    sb.Append(displayName);
                }

                return sb.ToString();
            };

            var systemNames = _permissionRepository.TableUntracked
                .Select(x => x.SystemName)
                .ToList()
                .ToDictionarySafe(x => x, nameSelector);

            return systemNames;
        }

        public virtual void InsertPermission(PermissionRecord permission)
        {
            Guard.NotNull(permission, nameof(permission));

            _permissionRepository.Insert(permission);

            _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
        }

        public virtual void UpdatePermission(PermissionRecord permission)
        {
            Guard.NotNull(permission, nameof(permission));

            _permissionRepository.Update(permission);

            _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
        }

        public virtual void DeletePermission(PermissionRecord permission)
        {
            if (permission != null)
            {
                _permissionRepository.Delete(permission);

                _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
            }
        }


        public virtual PermissionRoleMapping GetPermissionRoleMappingById(int mappingId)
        {
            if (mappingId == 0)
            {
                return null;
            }

            return _permissionMappingRepository.GetById(mappingId);
        }

        public virtual void InsertPermissionRoleMapping(PermissionRoleMapping mapping)
        {
            Guard.NotNull(mapping, nameof(mapping));

            _permissionMappingRepository.Insert(mapping);

            _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
        }

        public virtual void UpdatePermissionRoleMapping(PermissionRoleMapping mapping)
        {
            Guard.NotNull(mapping, nameof(mapping));

            _permissionMappingRepository.Update(mapping);

            _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
        }

        public virtual void DeletePermissionRoleMapping(PermissionRoleMapping mapping)
        {
            if (mapping != null)
            {
                _permissionMappingRepository.Delete(mapping);

                _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
            }
        }


        public virtual void InstallPermissions(IPermissionProvider2 permissionProvider)
        {
            Guard.NotNull(permissionProvider, nameof(permissionProvider));

            var permissions = permissionProvider.GetPermissions();
            if (!permissions.Any())
            {
                return;
            }

            using (var scope = new DbContextScope(_permissionRepository.Context, autoDetectChanges: false, autoCommit: false))
            {
                foreach (var permission in permissions)
                {
                    var newPermission = GetPermissionBySystemName(permission.SystemName);
                    if (newPermission == null)
                    {
                        newPermission = new PermissionRecord
                        {
                            Name = string.Empty,
                            SystemName = permission.SystemName,
                            Category = string.Empty
                        };

                        // Default customer role mappings.
                        var defaultPermissions = permissionProvider.GetDefaultPermissions();
                        foreach (var defaultPermission in defaultPermissions)
                        {
                            var customerRole = _customerService.Value.GetCustomerRoleBySystemName(defaultPermission.CustomerRoleSystemName);
                            if (customerRole == null)
                            {
                                customerRole = new CustomerRole
                                {
                                    Active = true,
                                    Name = defaultPermission.CustomerRoleSystemName,
                                    SystemName = defaultPermission.CustomerRoleSystemName
                                };
                                _customerService.Value.InsertCustomerRole(customerRole);
                            }

                            if (defaultPermission.PermissionRecords.Any(x => x.SystemName == newPermission.SystemName))
                            {
                                if (!customerRole.PermissionRoleMappings.Where(x => x.PermissionRecord.SystemName == newPermission.SystemName).Select(x => x.PermissionRecord).Any())
                                {
                                    newPermission.PermissionRoleMappings.Add(new PermissionRoleMapping
                                    {
                                        Allow = true,
                                        CustomerRoleId = customerRole.Id
                                    });
                                }
                            }
                        }

                        InsertPermission(newPermission);
                    }
                }

                scope.Commit();
                _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
            }
        }

        public virtual void UninstallPermissions(IPermissionProvider2 permissionProvider)
        {
            var permissions = permissionProvider.GetPermissions();
            var systemNames = new HashSet<string>(permissions.Select(x => x.SystemName));

            if (systemNames.Any())
            {
                var toDelete = _permissionRepository.Table
                    .Where(x => systemNames.Contains(x.SystemName))
                    .ToList();

                toDelete.Each(x => DeletePermission(x));

                _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
            }
        }


        public virtual bool Authorize(PermissionRecord permission)
        {
            return Authorize(permission, _workContext.CurrentCustomer);
        }

        public virtual bool Authorize(PermissionRecord permission, Customer customer)
        {
            if (permission == null || customer == null)
            {
                return false;
            }

            return Authorize(permission.SystemName, customer);
        }

        public virtual bool Authorize(string permissionSystemName)
        {
            return Authorize(permissionSystemName, _workContext.CurrentCustomer);
        }

        public virtual bool Authorize(string permissionSystemName, Customer customer)
        {
            if (string.IsNullOrEmpty(permissionSystemName))
            {
                return false;
            }

            // Rules for permission check:
            // - Direct permission takes precedence over indirect permission.
            // - "Deny" takes precedence over "allow".

            bool? directly = null;
            bool? indirectly = null;

            foreach (var role in customer.CustomerRoles.Where(x => x.Active))
            {
                var tree = GetPermissionTree(role);
                var node = tree.SelectNodeById(permissionSystemName);
                if (node == null)
                {
                    throw new SmartException($"Unknown permission \"{permissionSystemName}\".");
                }

                if (node.Value.Allow.HasValue)
                {
                    if (!(directly.HasValue && !directly.Value))
                    {
                        directly = node.Value.Allow;
                    }
                }
                else
                {
                    while (node != null && !node.Value.Allow.HasValue)
                    {
                        node = node.Parent;
                    }
                    if (node != null && node.Value.Allow.HasValue)
                    {
                        if (!(indirectly.HasValue && !indirectly.Value))
                        {
                            indirectly = node.Value.Allow;
                        }
                    }
                }
            }

            return directly ?? indirectly ?? false;
        }


        public virtual TreeNode<IPermissionNode> GetPermissionTree(CustomerRole role, bool addDisplayNames = false)
        {
            Guard.NotNull(role, nameof(role));

            var result = _cacheManager.Get(PERMISSION_TREE_KEY.FormatInvariant(role.Id), () =>
            {
                var root = new TreeNode<IPermissionNode>(new PermissionNode());

                var permissions = _permissionRepository.TableUntracked
                    .Expand(x => x.PermissionRoleMappings)
                    .Where(x => string.IsNullOrEmpty(x.Name))//GP: TODO, remove clause later
                    .ToList();

                AddChildItems(root, permissions, null, permission =>
                {
                    var mapping = permission.PermissionRoleMappings.FirstOrDefault(x => x.CustomerRoleId == role.Id);
                    return mapping?.Allow ?? null;
                });

                return root;
            });

            if (addDisplayNames)
            {
                var language = _workContext.WorkingLanguage;
                var resourcesLookup = GetDisplayNameLookup(language.Id);
                AddDisplayName(result, language.Id, resourcesLookup);
            }

            return result;
        }

        public virtual TreeNode<IPermissionNode> GetPermissionTree(Customer customer, bool addDisplayNames = false)
        {
            Guard.NotNull(customer, nameof(customer));

            var root = new TreeNode<IPermissionNode>(new PermissionNode());

            var permissions = _permissionRepository.TableUntracked
                .Where(x => string.IsNullOrEmpty(x.Name))//GP: TODO, remove clause later
                .ToList();

            AddChildItems(root, permissions, null, permission =>
            {
                return Authorize(permission.SystemName, customer);
            });

            if (addDisplayNames)
            {
                var language = _workContext.WorkingLanguage;
                var resourcesLookup = GetDisplayNameLookup(language.Id);
                AddDisplayName(root, language.Id, resourcesLookup);
            }

            return root;
        }


        private void AddChildItems(TreeNode<IPermissionNode> parentNode, List<PermissionRecord> permissions, string path, Func<PermissionRecord, bool?> allow)
        {
            if (parentNode == null)
            {
                return;
            }

            IEnumerable<PermissionRecord> entities = null;

            if (path == null)
            {
                entities = permissions.Where(x => x.SystemName.IndexOf('.') == -1);
            }
            else
            {
                var tmpPath = path.EnsureEndsWith(".");
                entities = permissions.Where(x => x.SystemName.StartsWith(tmpPath) && x.SystemName.IndexOf('.', tmpPath.Length) == -1);
            }

            foreach (var entity in entities)
            {
                var newNode = parentNode.Append(new PermissionNode
                {
                    PermissionRecordId = entity.Id,
                    Allow = allow(entity),     // null = inherit
                    SystemName = entity.SystemName
                }, entity.SystemName);

                AddChildItems(newNode, permissions, entity.SystemName, allow);
            }
        }

        private void AddDisplayName(TreeNode<IPermissionNode> node, int languageId, Dictionary<string, string> resourcesLookup)
        {
            var tokens = node.Value.SystemName.EmptyNull().ToLower().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var token = tokens.LastOrDefault();
            var displayName = GetDisplayName(token, languageId, resourcesLookup) ?? token ?? node.Value.SystemName;

            node.SetThreadMetadata("DisplayName", displayName);

            if (node.HasChildren)
            {
                node.Children.Each(x => AddDisplayName(x, languageId, resourcesLookup));
            }
        }

        private string GetDisplayName(string token, int languageId, Dictionary<string, string> resourcesLookup)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                // Try known token of default permissions.
                if (!_displayNameResourceKeys.TryGetValue(token, out var key) || !resourcesLookup.TryGetValue(key, out var name))
                {
                    // Unknown token. Try to find resource by name convention.
                    key = "Permissions.DisplayName." + token.Replace("-", "");

                    // Try resource provided by core.
                    name = _localizationService.GetResource(key, languageId, false, string.Empty, true);
                    if (name.IsEmpty())
                    {
                        // Try resource provided by plugin.
                        name = _localizationService.GetResource("Plugins." + key, languageId, false, string.Empty, true);
                    }
                }

                return name;
            }

            return null;
        }

        private Dictionary<string, string> GetDisplayNameLookup(int languageId)
        {
            var allKeys = _displayNameResourceKeys.Select(x => x.Value);

            // Load all known string resources in one go.
            var resourceQuery = _localizationService.All(languageId);
            var resourcesLookup = resourceQuery.Where(x => allKeys.Contains(x.ResourceName))
                .ToList()
                .ToDictionarySafe(x => x.ResourceName, x => x.ResourceValue);

            return resourcesLookup;
        }
    }
}
