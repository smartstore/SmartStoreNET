using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Collections;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Localization;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Services.Forums;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Infrastructure
{
    public partial class MyAccountMenu : IMenu
    {
        protected readonly ICommonServices _services;
        protected readonly Lazy<IOrderService> _orderService;
        protected readonly Lazy<IForumService> _forumService;
        protected readonly UrlHelper _urlHelper;
        protected readonly CustomerSettings _customerSettings;
        protected readonly OrderSettings _orderSettings;
        protected readonly RewardPointsSettings _rewardPointsSettings;
        protected readonly ForumSettings _forumSettings;

        protected TreeNode<MenuItem> _root;
        private TreeNode<MenuItem> _currentNode;
        private bool _currentNodeResolved;

        public MyAccountMenu(
            ICommonServices services,
            Lazy<IOrderService> orderService,
            Lazy<IForumService> forumService,
            UrlHelper urlHelper,
            CustomerSettings customerSettings,
            OrderSettings orderSettings,
            RewardPointsSettings rewardPointsSettings,
            ForumSettings forumSettings)
        {
            _services = services;
            _orderService = orderService;
            _forumService = forumService;
            _urlHelper = urlHelper;
            _customerSettings = customerSettings;
            _orderSettings = orderSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _forumSettings = forumSettings;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string Name => "MyAccount";

        public bool ApplyPermissions => true;

        public TreeNode<MenuItem> Root
        {
            get
            {
                if (_root == null)
                {
                    _root = BuildMenu();
                    _services.EventPublisher.Publish(new MenuBuiltEvent(Name, _root));
                }

                return _root;
            }
        }

        public void ResolveElementCounts(TreeNode<MenuItem> curNode, bool deep = false)
        {
            // Nothing to count.
        }

        public TreeNode<MenuItem> ResolveCurrentNode(ControllerContext context)
        {
            if (!_currentNodeResolved)
            {
                _currentNode = Root.SelectNode(x => x.Value.IsCurrent(context), true);
                _currentNodeResolved = true;
            }

            return _currentNode;
        }

        public IDictionary<string, TreeNode<MenuItem>> GetAllCachedMenus()
        {
            // No caching.
            return new Dictionary<string, TreeNode<MenuItem>>();
        }

        public void ClearCache()
        {
            // No caching.
        }

        protected virtual TreeNode<MenuItem> BuildMenu()
        {
            var store = _services.StoreContext.CurrentStore;
            var customer = _services.WorkContext.CurrentCustomer;

            var root = new TreeNode<MenuItem>(new MenuItem { Text = T("Account.Navigation") })
            {
                Id = Name
            };

            root.Append(new MenuItem
            {
                Id = "info",
                Text = T("Account.CustomerInfo"),
                Icon = "fal fa-user",
                Url = _urlHelper.Action("Info", "Customer", new { area = "" })
            });

            root.Append(new MenuItem
            {
                Id = "addresses",
                Text = T("Account.CustomerAddresses"),
                Icon = "fal fa-address-book",
                Url = _urlHelper.Action("Addresses", "Customer", new { area = "" })
            });

            root.Append(new MenuItem
            {
                Id = "orders",
                Text = T("Account.CustomerOrders"),
                Icon = "fal fa-file-invoice",
                Url = _urlHelper.Action("Orders", "Customer", new { area = "" })
            });

            if (_orderSettings.ReturnRequestsEnabled && _orderService.Value.SearchReturnRequests(store.Id, customer.Id, 0, null, 0, 1).TotalCount > 0)
            {
                root.Append(new MenuItem
                {
                    Id = "returnrequests",
                    Text = T("Account.CustomerReturnRequests"),
                    Icon = "fal fa-truck",
                    Url = _urlHelper.Action("ReturnRequests", "Customer", new { area = "" })
                });
            }

            if (!_customerSettings.HideDownloadableProductsTab)
            {
                root.Append(new MenuItem
                {
                    Id = "downloads",
                    Text = T("Account.DownloadableProducts"),
                    Icon = "fal fa-download",
                    Url = _urlHelper.Action("DownloadableProducts", "Customer", new { area = "" })
                });
            }

            if (!_customerSettings.HideBackInStockSubscriptionsTab)
            {
                root.Append(new MenuItem
                {
                    Id = "backinstock",
                    Text = T("Account.BackInStockSubscriptions"),
                    Icon = "fal fa-truck-loading",
                    Url = _urlHelper.Action("BackInStockSubscriptions", "Customer", new { area = "" })
                });
            }

            if (_rewardPointsSettings.Enabled)
            {
                root.Append(new MenuItem
                {
                    Id = "rewardpoints",
                    Text = T("Account.RewardPoints"),
                    Icon = "fal fa-certificate",
                    Url = _urlHelper.Action("RewardPoints", "Customer", new { area = "" })
                });
            }

            root.Append(new MenuItem
            {
                Id = "changepassword",
                Text = T("Account.ChangePassword"),
                Icon = "fal fa-unlock-alt",
                Url = _urlHelper.Action("ChangePassword", "Customer", new { area = "" })
            });

            if (_customerSettings.AllowCustomersToUploadAvatars)
            {
                root.Append(new MenuItem
                {
                    Id = "avatar",
                    Text = T("Account.Avatar"),
                    Icon = "fal fa-user-circle",
                    Url = _urlHelper.Action("Avatar", "Customer", new { area = "" })
                });
            }

            if (_forumSettings.ForumsEnabled && _forumSettings.AllowCustomersToManageSubscriptions)
            {
                root.Append(new MenuItem
                {
                    Id = "forumsubscriptions",
                    Text = T("Account.ForumSubscriptions"),
                    Icon = "fal fa-bell",
                    Url = _urlHelper.Action("ForumSubscriptions", "Customer", new { area = "" })
                });
            }

            if (_forumSettings.AllowPrivateMessages)
            {
                var numUnreadMessages = 0;

                if (_forumSettings.AllowPrivateMessages && !customer.IsGuest())
                {
                    var privateMessages = _forumService.Value.GetAllPrivateMessages(store.Id, 0, customer.Id, false, null, false, 0, 1);
                    numUnreadMessages = privateMessages.TotalCount;
                }

                root.Append(new MenuItem
                {
                    Id = "privatemessages",
                    Text = T("PrivateMessages.Inbox"),
                    Icon = "fal fa-envelope",
                    Url = _urlHelper.RouteUrl("PrivateMessages", new { tab = "inbox" }),
                    BadgeText = numUnreadMessages > 0 ? numUnreadMessages.ToString() : null,
                    BadgeStyle = BadgeStyle.Warning
                });
            }

            return root;
        }
    }
}