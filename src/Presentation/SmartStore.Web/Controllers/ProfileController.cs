﻿using System;
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
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Profile;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Web.Controllers
{
    [RequireHttpsByConfigAttribute(SslRequirement.No)]
    public partial class ProfileController : PublicControllerBase
    {
        private readonly IForumService _forumService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly MediaSettings _mediaSettings;

        public ProfileController(IForumService forumService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            ICountryService countryService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ForumSettings forumSettings,
            CustomerSettings customerSettings,
            MediaSettings mediaSettings)
        {
            this._forumService = forumService;
            this._localizationService = localizationService;
            this._pictureService = pictureService;
            this._countryService = countryService;
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._forumSettings = forumSettings;
            this._customerSettings = customerSettings;
            this._mediaSettings = mediaSettings;
        }

        public ActionResult Index(int? id, int? page)
        {
            var customerId = 0;
            if (id.HasValue)
            {
                customerId = id.Value;
            }

            var customer = _customerService.GetCustomerById(customerId);
            if (!_customerSettings.AllowViewingProfiles || (customer == null || customer.IsGuest()))
            {
				return HttpNotFound();
            }

            bool pagingPosts = false;
            int postsPage = 0;

            if (page.HasValue)
            {
                postsPage = page.Value;
                pagingPosts = true;
            }

            var name = customer.FormatUserName();
            var title = string.Format(_localizationService.GetResource("Profile.ProfileOf"), name);

            var model = new ProfileIndexModel()
            {
                ProfileTitle = title,
                PostsPage = postsPage,
                PagingPosts = pagingPosts,
                CustomerProfileId = customer.Id,
                ForumsEnabled = _forumSettings.ForumsEnabled
            };

            return View(model);
        }

        //profile info tab
        [ChildActionOnly]
        public ActionResult Info(int customerProfileId)
        {
            var customer = _customerService.GetCustomerById(customerProfileId);
            if (customer == null)
            {
				return HttpNotFound();
            }

            //avatar
            bool avatarEnabled = false;
            string avatarUrl = _pictureService.GetDefaultPictureUrl(_mediaSettings.AvatarPictureSize, PictureType.Avatar);
            if (_customerSettings.AllowCustomersToUploadAvatars)
            {
                avatarEnabled = true;

                var customerAvatarId = customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId);

                if (customerAvatarId != 0)
                {
                    avatarUrl = _pictureService.GetPictureUrl(customerAvatarId, _mediaSettings.AvatarPictureSize, false);
                }
                else
                {
                    if (!_customerSettings.DefaultAvatarEnabled)
                    {
                        avatarEnabled = false;
                    }
                }
            }

            //location
            bool locationEnabled = false;
            string location = string.Empty;
            if (_customerSettings.ShowCustomersLocation)
            {
                locationEnabled = true;

                var countryId = customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId);
                var country = _countryService.GetCountryById(countryId);
                if (country != null)
                {
                    location = country.GetLocalized(x => x.Name);
                }
                else
                {
                    locationEnabled = false;
                }
            }

            //private message
            bool pmEnabled = _forumSettings.AllowPrivateMessages && !customer.IsGuest();

            //total forum posts
            bool totalPostsEnabled = false;
            int totalPosts = 0;
            if (_forumSettings.ForumsEnabled && _forumSettings.ShowCustomersPostCount)
            {
                totalPostsEnabled = true;
                totalPosts = customer.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount);
            }

            //registration date
            bool joinDateEnabled = false;
            string joinDate = string.Empty;

            if (_customerSettings.ShowCustomersJoinDate)
            {
                joinDateEnabled = true;
                joinDate = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc).ToString("f");
            }

            //birth date
            bool dateOfBirthEnabled = false;
            string dateOfBirth = string.Empty;
            if (_customerSettings.DateOfBirthEnabled)
            {
                var dob = customer.GetAttribute<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
                if (dob.HasValue)
                {
                    dateOfBirthEnabled = true;
                    dateOfBirth = dob.Value.ToString("D");
                }
            }

            var model = new ProfileInfoModel()
            {
                CustomerProfileId = customer.Id,
                AvatarEnabled = avatarEnabled,
                AvatarUrl = avatarUrl,
                LocationEnabled = locationEnabled,
                Location = location,
                PMEnabled = pmEnabled,
                TotalPostsEnabled = totalPostsEnabled,
                TotalPosts = totalPosts.ToString(),
                JoinDateEnabled = joinDateEnabled,
                JoinDate = joinDate,
                DateOfBirthEnabled = dateOfBirthEnabled,
                DateOfBirth = dateOfBirth,
            };

            return PartialView(model);
        }

        //latest posts tab
        [ChildActionOnly]
        public ActionResult Posts(int customerProfileId, int page)
        {
            var customer = _customerService.GetCustomerById(customerProfileId);
            if (customer == null)
            {
				return HttpNotFound();
            }

            if (page > 0)
            {
                page -= 1;
            }

            var pageSize = _forumSettings.LatestCustomerPostsPageSize;

            var list = _forumService.GetAllPosts(0, customer.Id, string.Empty, false, page, pageSize);

            var latestPosts = new List<PostsModel>();

            foreach (var forumPost in list)
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

                latestPosts.Add(new PostsModel()
                {
                    ForumTopicId = forumPost.TopicId,
                    ForumTopicTitle = forumPost.ForumTopic.Subject,
                    ForumTopicSlug = forumPost.ForumTopic.GetSeName(),
                    ForumPostText = forumPost.FormatPostText(),
                    Posted = posted
                });
            }

            ViewData["PagerRouteValues"] = new RouteValues { page = page, id = customerProfileId };

            var model = new ProfilePostsModel(list)
            {
                Posts = latestPosts,
            };

            return PartialView(model);
        }
    }
}
