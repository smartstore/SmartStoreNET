using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Forums;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Seo;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Profile;

namespace SmartStore.Web.Controllers
{
    [RewriteUrl(SslRequirement.No)]
    public partial class ProfileController : PublicControllerBase
    {
        private readonly IForumService _forumService;
        private readonly IMediaService _mediaService;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly MediaSettings _mediaSettings;

        public ProfileController(
            IForumService forumService,
            IMediaService mediaService,
            ICountryService countryService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IDateTimeHelper dateTimeHelper,
            ForumSettings forumSettings,
            CustomerSettings customerSettings,
            MediaSettings mediaSettings)
        {
            _forumService = forumService;
            _mediaService = mediaService;
            _countryService = countryService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _dateTimeHelper = dateTimeHelper;
            _forumSettings = forumSettings;
            _customerSettings = customerSettings;
            _mediaSettings = mediaSettings;
        }

        public ActionResult Index(int? id, int? page)
        {
            var customer = _customerService.GetCustomerById(id ?? 0);
            if (!_customerSettings.AllowViewingProfiles || (customer == null || customer.IsGuest()))
            {
                return HttpNotFound();
            }

            var name = customer.FormatUserName(_customerSettings, T, true);

            var model = new ProfileIndexModel
            {
                Id = customer.Id,
                ProfileTitle = T("Profile.ProfileOf", name),
                PostsPage = page ?? 0,
                PagingPosts = page.HasValue,
                ForumsEnabled = _forumSettings.ForumsEnabled
            };

            return View(model);
        }

        // Profile info tab.
        [ChildActionOnly]
        public ActionResult Info(int id)
        {
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            var model = new ProfileInfoModel
            {
                Id = customer.Id
            };

            model.Avatar = customer.ToAvatarModel(_genericAttributeService, _customerSettings, _mediaSettings, null, true);

            // Location.
            if (_customerSettings.ShowCustomersLocation)
            {
                model.LocationEnabled = true;

                var country = _countryService.GetCountryById(customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId, _genericAttributeService));
                if (country != null)
                {
                    model.Location = country.GetLocalized(x => x.Name);
                }
                else
                {
                    model.LocationEnabled = false;
                }
            }

            // Private message.
            model.PMEnabled = _forumSettings.AllowPrivateMessages && !customer.IsGuest();

            // Total forum posts.
            if (_forumSettings.ForumsEnabled && _forumSettings.ShowCustomersPostCount)
            {
                model.TotalPostsEnabled = true;
                model.TotalPosts = customer.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount, _genericAttributeService);
            }

            // Registration date.
            if (_customerSettings.ShowCustomersJoinDate)
            {
                model.JoinDateEnabled = true;
                model.JoinDate = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc).ToString("f");
            }

            // Birth date.
            if (_customerSettings.DateOfBirthEnabled && customer.BirthDate.HasValue)
            {
                model.DateOfBirthEnabled = true;
                model.DateOfBirth = customer.BirthDate.Value.ToString("D");
            }

            return PartialView(model);
        }

        // Latest posts tab.
        [ChildActionOnly]
        public ActionResult Posts(int id, int page)
        {
            var customer = _customerService.GetCustomerById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            if (page > 0)
            {
                page -= 1;
            }

            var posts = _forumService.GetAllPosts(0, customer.Id, false, page, _forumSettings.LatestCustomerPostsPageSize);
            var latestPosts = new List<PostsModel>();

            foreach (var forumPost in posts)
            {
                var posted = string.Empty;
                if (_forumSettings.RelativeDateTimeFormattingEnabled)
                {
                    posted = forumPost.CreatedOnUtc.RelativeFormat(true, "f");
                }
                else
                {
                    posted = _dateTimeHelper.ConvertToUserTime(forumPost.CreatedOnUtc, DateTimeKind.Utc).ToString("f");
                }

                latestPosts.Add(new PostsModel
                {
                    ForumTopicId = forumPost.TopicId,
                    ForumTopicTitle = forumPost.ForumTopic.Subject,
                    ForumTopicSlug = forumPost.ForumTopic.GetSeName(),
                    ForumPostText = forumPost.FormatPostText(),
                    Posted = posted
                });
            }

            ViewData["PagerRouteValues"] = new RouteValues { page = page, id = id };

            var model = new ProfilePostsModel(posts)
            {
                Posts = latestPosts,
            };

            return PartialView(model);
        }
    }
}
