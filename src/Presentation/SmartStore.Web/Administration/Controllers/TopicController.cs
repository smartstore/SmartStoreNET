using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Topics;
using SmartStore.Core;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Security;
using SmartStore.Services.Cms;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class TopicController : AdminControllerBase
    {
        private readonly ITopicService _topicService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IAclService _aclService;
        private readonly IMenuStorage _menuStorage;
        private readonly ILinkResolver _linkResolver;

        public TopicController(
            ITopicService topicService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IAclService aclService,
            IMenuStorage menuStorage,
            ILinkResolver linkResolver)
        {
            _topicService = topicService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _aclService = aclService;
            _menuStorage = menuStorage;
            _linkResolver = linkResolver;
        }

        private void UpdateLocales(Topic topic, TopicModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(topic, x => x.ShortTitle, localized.ShortTitle, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(topic, x => x.Title, localized.Title, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(topic, x => x.Intro, localized.Intro, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(topic, x => x.Body, localized.Body, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(topic, x => x.MetaKeywords, localized.MetaKeywords, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(topic, x => x.MetaDescription, localized.MetaDescription, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(topic, x => x.MetaTitle, localized.MetaTitle, localized.LanguageId);

                var seName = topic.ValidateSeName(localized.SeName, localized.Title.NullEmpty() ?? localized.ShortTitle, false, localized.LanguageId);
                _urlRecordService.SaveSlug(topic, seName, localized.LanguageId);
            }
        }

        private void PrepareStoresMappingModel(TopicModel model, Topic topic, bool excludeProperties)
        {
            Guard.NotNull(model, nameof(model));

            if (!excludeProperties)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(topic);
            }
        }

        private void PrepareAclModel(TopicModel model, Topic topic, bool excludeProperties)
        {
            Guard.NotNull(model, nameof(model));

            if (!excludeProperties)
            {
                model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccessTo(topic);
            }
        }

        private void AddCookieTypes(TopicModel model, int? selectedType = 0)
        {
            model.AvailableCookieTypes.Add(new SelectListItem()
            {
                Text = "Required",
                Value = ((int)CookieType.Required).ToString(),
                Selected = CookieType.Required == (CookieType?)selectedType
            });
            model.AvailableCookieTypes.Add(new SelectListItem()
            {
                Text = "Analytics",
                Value = ((int)CookieType.Analytics).ToString(),
                Selected = CookieType.Analytics == (CookieType?)selectedType
            });
            model.AvailableCookieTypes.Add(new SelectListItem()
            {
                Text = "ThirdParty",
                Value = ((int)CookieType.ThirdParty).ToString(),
                Selected = CookieType.ThirdParty == (CookieType?)selectedType
            });
        }

        private string GetTopicUrl(Topic topic)
        {
            string url = null;

            try
            {
                if (topic.LimitedToStores)
                {
                    var storeMappings = _storeMappingService.GetStoreMappings(topic);
                    if (storeMappings.FirstOrDefault(x => x.StoreId == Services.StoreContext.CurrentStore.Id) == null)
                    {
                        var storeMapping = storeMappings.FirstOrDefault();
                        if (storeMapping != null)
                        {
                            var store = Services.StoreService.GetStoreById(storeMapping.StoreId);
                            if (store != null)
                            {
                                var baseUri = new Uri(Services.StoreService.GetHost(store));
                                url = baseUri.GetLeftPart(UriPartial.Authority) + Url.RouteUrl("Topic", new { SeName = topic.GetSeName() });
                            }
                        }
                    }
                }

                if (url.IsEmpty())
                {
                    url = Url.RouteUrl("Topic", new { SeName = topic.GetSeName() }, Request.Url.Scheme);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return url;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Cms.Topic.Read)]
        public ActionResult List()
        {
            var model = new TopicListModel();

            foreach (var s in Services.StoreService.GetAllStores())
            {
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            }

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Topic.Read)]
        public ActionResult List(GridCommand command, TopicListModel model)
        {
            var gridModel = new GridModel<TopicModel>();
            var topics = _topicService.GetAllTopics(model.SearchStoreId, command.Page - 1, command.PageSize, true).AlterQuery(q =>
            {
                var q2 = q;

                if (model.SystemName.HasValue())
                    q2 = q2.Where(x => x.SystemName.Contains(model.SystemName));

                if (model.Title.HasValue())
                    q2 = q2.Where(x => x.Title.Contains(model.Title) || x.ShortTitle.Contains(model.Title));

                if (model.RenderAsWidget.HasValue)
                    q2 = q2.Where(x => x.RenderAsWidget == model.RenderAsWidget.Value);

                if (model.WidgetZone.HasValue())
                    q2 = q2.Where(x => x.WidgetZone.Contains(model.WidgetZone));

                return q2.OrderBy(x => x.SystemName);
            });

            gridModel.Data = topics.Select(x =>
            {
                var item = x.ToModel();
                item.WidgetZone = x.WidgetZone.SplitSafe(",");
                // Otherwise maxJsonLength could be exceeded.
                item.Body = "";
                return item;
            });

            gridModel.Total = topics.TotalCount;

            return new JsonResult
            {
                MaxJsonLength = int.MaxValue,
                Data = gridModel
            };
        }

        [Permission(Permissions.Cms.Topic.Create)]
        public ActionResult Create()
        {
            var model = new TopicModel();

            PrepareStoresMappingModel(model, null, false);
            PrepareAclModel(model, null, false);
            AddLocales(_languageService, model.Locales);
            AddCookieTypes(model);

            model.TitleTag = "h1";

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Topic.Create)]
        public ActionResult Create(TopicModel model, bool continueEditing, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                if (!model.IsPasswordProtected)
                {
                    model.Password = null;
                }

                var topic = model.ToEntity();

                if (model.WidgetZone != null)
                {
                    topic.WidgetZone = string.Join(",", model.WidgetZone);
                }

                topic.CookieType = (CookieType?)model.CookieType;

                _topicService.InsertTopic(topic);

                model.SeName = topic.ValidateSeName(model.SeName, topic.Title.NullEmpty() ?? topic.SystemName, true);
                _urlRecordService.SaveSlug(topic, model.SeName, 0);

                SaveStoreMappings(topic, model.SelectedStoreIds);
                SaveAclMappings(topic, model.SelectedCustomerRoleIds);
                UpdateLocales(topic, model);
                AddCookieTypes(model, model.CookieType);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, topic, form));

                NotifySuccess(T("Admin.ContentManagement.Topics.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = topic.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.
            PrepareStoresMappingModel(model, null, true);
            PrepareAclModel(model, null, true);

            return View(model);
        }

        [Permission(Permissions.Cms.Topic.Read)]
        public ActionResult Edit(int id)
        {
            var topic = _topicService.GetTopicById(id);
            if (topic == null)
            {
                return RedirectToAction("List");
            }

            var model = topic.ToModel();
            model.Url = GetTopicUrl(topic);
            model.WidgetZone = topic.WidgetZone.SplitSafe(",");
            model.CookieType = (int?)topic.CookieType;

            PrepareStoresMappingModel(model, topic, false);
            PrepareAclModel(model, topic, false);
            AddCookieTypes(model, model.CookieType);

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.ShortTitle = topic.GetLocalized(x => x.ShortTitle, languageId, false, false);
                locale.Title = topic.GetLocalized(x => x.Title, languageId, false, false);
                locale.Intro = topic.GetLocalized(x => x.Intro, languageId, false, false);
                locale.Body = topic.GetLocalized(x => x.Body, languageId, false, false);
                locale.MetaKeywords = topic.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = topic.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = topic.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = topic.GetSeName(languageId, false, false);
            });

            // Get menu links.
            IPagedList<MenuRecord> menus = null;
            var pageIndex = 0;

            do
            {
                menus = _menuStorage.GetAllMenus(null, 0, true, pageIndex++, 500);

                foreach (var menu in menus)
                {
                    foreach (var item in menu.Items.Where(x => x.ProviderName != null && x.ProviderName == "entity"))
                    {
                        var link = _linkResolver.Resolve(item.Model);
                        if (link.Type == LinkType.Topic && link.Id == topic.Id)
                        {
                            var url = Url.Action("EditItem", "Menu", new { id = item.Id, area = "Admin" });

                            var label = string.Concat(
                                menu.Title.NullEmpty() ?? menu.SystemName.NullEmpty() ?? "".NaIfEmpty(),
                                " » ",
                                item.Title.NullEmpty() ?? link.Label.NullEmpty() ?? "".NaIfEmpty());

                            model.MenuLinks[url] = label;
                        }
                    }
                }
            }
            while (menus.HasNextPage);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Topic.Update)]
        public ActionResult Edit(TopicModel model, bool continueEditing, FormCollection form)
        {
            var topic = _topicService.GetTopicById(model.Id);
            if (topic == null)
            {
                return RedirectToAction("List");
            }

            if (!model.IsPasswordProtected)
            {
                model.Password = null;
            }

            if (ModelState.IsValid)
            {
                topic = model.ToEntity(topic);

                if (model.WidgetZone != null)
                {
                    topic.WidgetZone = string.Join(",", model.WidgetZone);
                }
                
                topic.CookieType = (CookieType?)model.CookieType;

                _topicService.UpdateTopic(topic);

                model.SeName = topic.ValidateSeName(model.SeName, topic.Title.NullEmpty() ?? topic.SystemName, true);
                _urlRecordService.SaveSlug(topic, model.SeName, 0);

                SaveStoreMappings(topic, model.SelectedStoreIds);
                SaveAclMappings(topic, model.SelectedCustomerRoleIds);
                UpdateLocales(topic, model);
                AddCookieTypes(model, model.CookieType);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, topic, form));

                NotifySuccess(T("Admin.ContentManagement.Topics.Updated"));
                return continueEditing ? RedirectToAction("Edit", topic.Id) : RedirectToAction("List");
            }
            else
            {
                // Chrome spat out an error message after validation with this rule .Must(u => u.IsEmpty() || !u.Any(x => char.IsWhiteSpace(x)))
                HttpContext.Response.AddHeader("X-XSS-Protection", "0");
            }

            // If we got this far, something failed, redisplay form.
            model.Url = GetTopicUrl(topic);
            PrepareStoresMappingModel(model, topic, true);
            PrepareAclModel(model, topic, true);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Topic.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var topic = _topicService.GetTopicById(id);
            if (topic == null)
            {
                return HttpNotFound();
            }

            if (topic.IsSystemTopic)
            {
                NotifyError(T("Admin.ContentManagement.Topics.CannotBeDeleted"));
                return RedirectToAction("Edit", new { id = topic.Id });
            }

            _topicService.DeleteTopic(topic);

            NotifySuccess(T("Admin.ContentManagement.Topics.Deleted"));
            return RedirectToAction("List");
        }

        // AJAX.
        public ActionResult AllTopics(string label, int selectedId, bool includeWidgets = false, bool includeHomePage = false)
        {
            var query = from x in _topicService.GetAllTopics(showHidden: true).SourceQuery
                        where includeWidgets || !x.RenderAsWidget
                        select x;

            var topics = query.ToList();

            var list = topics
                .Select(x =>
                {
                    var item = new ChoiceListItem
                    {
                        Id = x.Id.ToString(),
                        Text = x.GetLocalized(y => y.Title).Value.NullEmpty() ?? x.SystemName,
                        Selected = x.Id == selectedId
                    };

                    if (!item.Text.IsCaseInsensitiveEqual(x.SystemName))
                    {
                        item.Description = x.SystemName;
                    }

                    return item;
                })
                .ToList();

            if (label.HasValue())
            {
                list.Insert(0, new ChoiceListItem { Id = "0", Text = label, Selected = false });
            }
            if (includeHomePage)
            {
                list.Insert(0, new ChoiceListItem { Id = "-10", Text = T("Admin.ContentManagement.Homepage").Text, Selected = false });
            }

            return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
