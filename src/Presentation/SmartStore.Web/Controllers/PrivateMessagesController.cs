using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Logging;
using SmartStore.Services.Customers;
using SmartStore.Services.Forums;
using SmartStore.Services.Helpers;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.PrivateMessages;

namespace SmartStore.Web.Controllers
{
	[RequireHttpsByConfigAttribute(SslRequirement.Yes)]
    public partial class PrivateMessagesController : PublicControllerBase
    {
        #region Fields

        private readonly IForumService _forumService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Constructors

        public PrivateMessagesController(IForumService forumService,
            ICustomerService customerService,
			ICustomerActivityService customerActivityService,
			IWorkContext workContext,
			IStoreContext storeContext,
            IDateTimeHelper dateTimeHelper,
            ForumSettings forumSettings,
			CustomerSettings customerSettings)
        {
            this._forumService = forumService;
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._dateTimeHelper = dateTimeHelper;
            this._forumSettings = forumSettings;
            this._customerSettings = customerSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        private bool AllowPrivateMessages()
        {
            return _forumSettings.AllowPrivateMessages;
        }

        #endregion

        #region Methods

        public ActionResult Index(int? page, string tab)
        {
            if (!AllowPrivateMessages())
            {
				return HttpNotFound();
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return new HttpUnauthorizedResult();
            }

            int inboxPage = 0;
            int sentItemsPage = 0;
            bool sentItemsTabSelected = false;

            switch (tab)
            {
                case "inbox":
                    if (page.HasValue)
                    {
                        inboxPage = page.Value;
                    }
                    break;
                case "sent":
                    if (page.HasValue)
                    {
                        sentItemsPage = page.Value;
                    }
                    sentItemsTabSelected = true;
                    break;
                default:
                    break;
            }

            var model = new PrivateMessageIndexModel
            {
                InboxPage = inboxPage,
                SentItemsPage = sentItemsPage,
                SentItemsTabSelected = sentItemsTabSelected
            };

            return View(model);
        }

        //inbox tab
        [ChildActionOnly]
        public ActionResult Inbox(int page, string tab)
        {
            if (page > 0)
            {
                page -= 1;
            }

            var pageSize = _forumSettings.PrivateMessagesPageSize;

			var list = _forumService.GetAllPrivateMessages(
				_storeContext.CurrentStore.Id,
				 0, 
				 _workContext.CurrentCustomer.Id, 
				 null, null, false, string.Empty, 
				 page, pageSize);

            var inbox = new List<PrivateMessageModel>();

            foreach (var pm in list)
            {
                inbox.Add(new PrivateMessageModel
                {
                    Id = pm.Id,
                    FromCustomerId = pm.FromCustomer.Id,
                    CustomerFromName = pm.FromCustomer.FormatUserName(),
                    AllowViewingFromProfile = _customerSettings.AllowViewingProfiles && pm.FromCustomer != null && !pm.FromCustomer.IsGuest(),
                    ToCustomerId = pm.ToCustomer.Id,
                    CustomerToName = pm.ToCustomer.FormatUserName(),
                    AllowViewingToProfile = _customerSettings.AllowViewingProfiles && pm.ToCustomer != null && !pm.ToCustomer.IsGuest(),
                    Subject = pm.Subject,
                    Message = pm.Text,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(pm.CreatedOnUtc, DateTimeKind.Utc),
                    IsRead = pm.IsRead,
                });
            }

            ViewData["PagerRouteValues"] = new PrivateMessageRouteValues { page = page, tab = tab };

            var model = new PrivateMessageListModel(list)
            {
                Messages = inbox
            };

            return PartialView(model);
        }

        //sent items tab
        [ChildActionOnly]
        public ActionResult SentItems(int page, string tab)
        {
            if (page > 0)
            {
                page -= 1;
            }

            var pageSize = _forumSettings.PrivateMessagesPageSize;

			var list = _forumService.GetAllPrivateMessages(_storeContext.CurrentStore.Id,
				_workContext.CurrentCustomer.Id, 0, null, false, null, string.Empty, page, pageSize);

            var sentItems = new List<PrivateMessageModel>();

            foreach (var pm in list)
            {
                sentItems.Add(new PrivateMessageModel
                {
                    Id = pm.Id,
                    FromCustomerId = pm.FromCustomer.Id,
                    CustomerFromName = pm.FromCustomer.FormatUserName(),
                    AllowViewingFromProfile = _customerSettings.AllowViewingProfiles && pm.FromCustomer != null && !pm.FromCustomer.IsGuest(),
                    ToCustomerId = pm.ToCustomer.Id,
                    CustomerToName = pm.ToCustomer.FormatUserName(),
                    AllowViewingToProfile = _customerSettings.AllowViewingProfiles && pm.ToCustomer != null && !pm.ToCustomer.IsGuest(),
                    Subject = pm.Subject,
                    Message = pm.Text,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(pm.CreatedOnUtc, DateTimeKind.Utc),
                    IsRead = pm.IsRead,
                });
            }

            ViewData["PagerRouteValues"] = new PrivateMessageRouteValues { page = page, tab = tab };

            var model = new PrivateMessageListModel(list)
            {
                Messages = sentItems
            };

            return PartialView(model);
        }

        [HttpPost, FormValueRequired("delete-inbox"), ActionName("InboxUpdate")]
        public ActionResult DeleteInboxPM(FormCollection formCollection)
        {
            foreach (var key in formCollection.AllKeys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("pm", StringComparison.InvariantCultureIgnoreCase))
                {
                    var id = key.Replace("pm", "").Trim();
                    int privateMessageId = 0;

                    if (Int32.TryParse(id, out privateMessageId))
                    {
                        var pm = _forumService.GetPrivateMessageById(privateMessageId);
                        if (pm != null)
                        {
                            if (pm.ToCustomerId == _workContext.CurrentCustomer.Id)
                            {
                                pm.IsDeletedByRecipient = true;
                                _forumService.UpdatePrivateMessage(pm);
                            }
                        }
                    }
                }
            }
			return RedirectToAction("Index");
		}

        [HttpPost, FormValueRequired("mark-unread"), ActionName("InboxUpdate")]
        public ActionResult MarkUnread(FormCollection formCollection)
        {
            foreach (var key in formCollection.AllKeys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("pm", StringComparison.InvariantCultureIgnoreCase))
                {
                    var id = key.Replace("pm", "").Trim();
                    int privateMessageId = 0;

                    if (Int32.TryParse(id, out privateMessageId))
                    {
                        var pm = _forumService.GetPrivateMessageById(privateMessageId);
                        if (pm != null)
                        {
                            if (pm.ToCustomerId == _workContext.CurrentCustomer.Id)
                            {
                                pm.IsRead = false;
                                _forumService.UpdatePrivateMessage(pm);
                            }
                        }
                    }
                }
            }
			return RedirectToAction("Index");
        }

        //updates sent items (deletes PrivateMessages)
        [HttpPost, FormValueRequired("delete-sent"), ActionName("SentUpdate")]
        public ActionResult DeleteSentPM(FormCollection formCollection)
        {
            foreach (var key in formCollection.AllKeys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("si", StringComparison.InvariantCultureIgnoreCase))
                {
                    var id = key.Replace("si", "").Trim();
                    int privateMessageId = 0;

                    if (Int32.TryParse(id, out privateMessageId))
                    {
                        PrivateMessage pm = _forumService.GetPrivateMessageById(privateMessageId);
                        if (pm != null)
                        {
                            if (pm.FromCustomerId == _workContext.CurrentCustomer.Id)
                            {
                                pm.IsDeletedByAuthor = true;
                                _forumService.UpdatePrivateMessage(pm);
                            }
                        }
                    }
                }

            }
            return RedirectToRoute("PrivateMessages", new { tab = "sent" });
        }

        public ActionResult Send(int id /* toCustomerId */, int? replyToMessageId)
        {
            if (!AllowPrivateMessages())
            {
				return HttpNotFound();
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return new HttpUnauthorizedResult();
            }

            var customerTo = _customerService.GetCustomerById(id);

            if (customerTo == null || customerTo.IsGuest())
            {
				return RedirectToAction("Index");
			}

            var model = new SendPrivateMessageModel();
            model.ToCustomerId = customerTo.Id;
            model.CustomerToName = customerTo.FormatUserName();
            model.AllowViewingToProfile = _customerSettings.AllowViewingProfiles && !customerTo.IsGuest();

            if (replyToMessageId.HasValue)
            {
                var replyToPM = _forumService.GetPrivateMessageById(replyToMessageId.Value);
                if (replyToPM == null)
                {
					return RedirectToAction("Index");
				}

                if (replyToPM.ToCustomerId == _workContext.CurrentCustomer.Id || replyToPM.FromCustomerId == _workContext.CurrentCustomer.Id)
                {
                    model.ReplyToMessageId = replyToPM.Id;
                    model.Subject = string.Format("Re: {0}", replyToPM.Subject);
                }
                else
                {
					return RedirectToAction("Index");
				}
            }
			return View(model);
        }

        [HttpPost]
        public ActionResult Send(SendPrivateMessageModel model)
        {
            if (!AllowPrivateMessages())
            {
				return HttpNotFound();
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return new HttpUnauthorizedResult();
            }

            Customer toCustomer = null;
            var replyToPM = _forumService.GetPrivateMessageById(model.ReplyToMessageId);
            if (replyToPM != null)
            {
                if (replyToPM.ToCustomerId == _workContext.CurrentCustomer.Id || replyToPM.FromCustomerId == _workContext.CurrentCustomer.Id)
                {
                    toCustomer = replyToPM.FromCustomer;
                }
                else
                {
					return RedirectToAction("Index");
				}
            }
            else
            {
                toCustomer = _customerService.GetCustomerById(model.ToCustomerId);
            }

            if (toCustomer == null || toCustomer.IsGuest())
            {
				return RedirectToAction("Index");
			}
            model.ToCustomerId = toCustomer.Id;
            model.CustomerToName = toCustomer.FormatUserName();
            model.AllowViewingToProfile = _customerSettings.AllowViewingProfiles && !toCustomer.IsGuest();

            if (ModelState.IsValid)
            {
                try
                {
                    string subject = model.Subject;
                    if (_forumSettings.PMSubjectMaxLength > 0 && subject.Length > _forumSettings.PMSubjectMaxLength)
                    {
                        subject = subject.Substring(0, _forumSettings.PMSubjectMaxLength);
                    }

                    var text = model.Message;
                    if (_forumSettings.PMTextMaxLength > 0 && text.Length > _forumSettings.PMTextMaxLength)
                    {
                        text = text.Substring(0, _forumSettings.PMTextMaxLength);
                    }

                    var nowUtc = DateTime.UtcNow;

                    var privateMessage = new PrivateMessage
                    {
						StoreId = _storeContext.CurrentStore.Id,
                        ToCustomerId = toCustomer.Id,
                        FromCustomerId = _workContext.CurrentCustomer.Id,
                        Subject = subject,
                        Text = text,
                        IsDeletedByAuthor = false,
                        IsDeletedByRecipient = false,
                        IsRead = false,
                        CreatedOnUtc = nowUtc
                    };

                    _forumService.InsertPrivateMessage(privateMessage);

                    //activity log
                    _customerActivityService.InsertActivity("PublicStore.SendPM", T("ActivityLog.PublicStore.SendPM", toCustomer.Email));

					return RedirectToAction("Index", new { tab = "sent" });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(model);
        }

        public ActionResult View(int id /* privateMessageId */)
        {
            if (!AllowPrivateMessages())
            {
				return HttpNotFound();
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return new HttpUnauthorizedResult();
            }

            var pm = _forumService.GetPrivateMessageById(id);
            if (pm != null)
            {
                if (pm.ToCustomerId != _workContext.CurrentCustomer.Id && pm.FromCustomerId != _workContext.CurrentCustomer.Id)
                {
					return RedirectToAction("Index");
				}

                if (!pm.IsRead && pm.ToCustomerId == _workContext.CurrentCustomer.Id)
                {
                    pm.IsRead = true;
                    _forumService.UpdatePrivateMessage(pm);
                }
            }
            else
            {
				return RedirectToAction("Index");
			}

            var model = new PrivateMessageModel
            {
                Id = pm.Id,
                FromCustomerId = pm.FromCustomer.Id,
                CustomerFromName = pm.FromCustomer.FormatUserName(),
                AllowViewingFromProfile = _customerSettings.AllowViewingProfiles && pm.FromCustomer != null && !pm.FromCustomer.IsGuest(),
                ToCustomerId = pm.ToCustomer.Id,
                CustomerToName = pm.ToCustomer.FormatUserName(),
                AllowViewingToProfile = _customerSettings.AllowViewingProfiles && pm.ToCustomer != null && !pm.ToCustomer.IsGuest(),
                Subject = pm.Subject,
                Message = pm.FormatPrivateMessageText(),
                CreatedOn = _dateTimeHelper.ConvertToUserTime(pm.CreatedOnUtc, DateTimeKind.Utc),
                IsRead = pm.IsRead,
            };

            return View(model);
        }

        public ActionResult Delete(int id /* privateMessageId */)
        {
            if (!AllowPrivateMessages())
            {
				return HttpNotFound();
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return new HttpUnauthorizedResult();
            }

            var pm = _forumService.GetPrivateMessageById(id);
            if (pm != null)
            {
                if (pm.FromCustomerId == _workContext.CurrentCustomer.Id)
                {
                    pm.IsDeletedByAuthor = true;
                    _forumService.UpdatePrivateMessage(pm);
                }

                if (pm.ToCustomerId == _workContext.CurrentCustomer.Id)
                {
                    pm.IsDeletedByRecipient = true;
                    _forumService.UpdatePrivateMessage(pm);
                }
            }
			return RedirectToAction("Index");
		}

        #endregion
    }
}
