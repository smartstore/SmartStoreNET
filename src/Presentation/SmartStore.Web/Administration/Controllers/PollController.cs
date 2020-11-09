using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Polls;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Polls;
using SmartStore.Core.Security;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Polls;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class PollController : AdminControllerBase
    {
        private readonly IPollService _pollService;
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly CustomerSettings _customerSettings;

        public PollController(
            IPollService pollService,
            ILanguageService languageService,
            IDateTimeHelper dateTimeHelper,
            AdminAreaSettings adminAreaSettings,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            CustomerSettings customerSettings)
        {
            _pollService = pollService;
            _languageService = languageService;
            _dateTimeHelper = dateTimeHelper;
            _adminAreaSettings = adminAreaSettings;
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _customerSettings = customerSettings;
        }

        private void PreparePollModel(PollModel model, Poll poll, bool excludeProperties)
        {
            Guard.NotNull(model, nameof(model));

            if (!excludeProperties)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(poll);
            }

            model.UsernamesEnabled = _customerSettings.CustomerLoginType != CustomerLoginType.Email;
            model.GridPageSize = _adminAreaSettings.GridPageSize;

            model.AvailableLanguages = _languageService.GetAllLanguages(true)
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToList();
        }

        #region Polls

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Cms.Poll.Read)]
        public ActionResult List()
        {
            var polls = _pollService.GetPolls(0, false, 0, _adminAreaSettings.GridPageSize, true);
            var gridModel = new GridModel<PollModel>
            {
                Data = polls.Select(x =>
                {
                    var m = x.ToModel();
                    if (x.StartDateUtc.HasValue)
                        m.StartDate = _dateTimeHelper.ConvertToUserTime(x.StartDateUtc.Value, DateTimeKind.Utc);
                    if (x.EndDateUtc.HasValue)
                        m.EndDate = _dateTimeHelper.ConvertToUserTime(x.EndDateUtc.Value, DateTimeKind.Utc);
                    m.LanguageName = x.Language.Name;
                    return m;
                }),
                Total = polls.TotalCount
            };
            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Poll.Read)]
        public ActionResult List(GridCommand command)
        {
            var gridModel = new GridModel<PollModel>();
            var polls = _pollService.GetPolls(0, false, command.Page - 1, command.PageSize, true);

            gridModel.Data = polls.Select(x =>
            {
                var m = x.ToModel();

                if (x.StartDateUtc.HasValue)
                    m.StartDate = _dateTimeHelper.ConvertToUserTime(x.StartDateUtc.Value, DateTimeKind.Utc);

                if (x.EndDateUtc.HasValue)
                    m.EndDate = _dateTimeHelper.ConvertToUserTime(x.EndDateUtc.Value, DateTimeKind.Utc);

                m.LanguageName = x.Language.Name;

                return m;
            });

            gridModel.Total = polls.TotalCount;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Cms.Poll.Create)]
        public ActionResult Create()
        {
            var model = new PollModel
            {
                Published = true,
                ShowOnHomePage = true
            };

            PreparePollModel(model, null, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Poll.Create)]
        public ActionResult Create(PollModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var poll = model.ToEntity();
                poll.StartDateUtc = model.StartDate;
                poll.EndDateUtc = model.EndDate;

                _pollService.InsertPoll(poll);

                SaveStoreMappings(poll, model.SelectedStoreIds);

                NotifySuccess(T("Admin.ContentManagement.Polls.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = poll.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.		
            PreparePollModel(model, null, true);
            return View(model);
        }

        [Permission(Permissions.Cms.Poll.Read)]
        public ActionResult Edit(int id)
        {
            var poll = _pollService.GetPollById(id);
            if (poll == null)
            {
                return RedirectToAction("List");
            }

            var model = poll.ToModel();
            model.StartDate = poll.StartDateUtc;
            model.EndDate = poll.EndDateUtc;

            PreparePollModel(model, poll, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Poll.Update)]
        public ActionResult Edit(PollModel model, bool continueEditing)
        {
            var poll = _pollService.GetPollById(model.Id);
            if (poll == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                poll = model.ToEntity(poll);
                poll.StartDateUtc = model.StartDate;
                poll.EndDateUtc = model.EndDate;

                _pollService.UpdatePoll(poll);

                SaveStoreMappings(poll, model.SelectedStoreIds);

                NotifySuccess(T("Admin.ContentManagement.Polls.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = poll.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.
            PreparePollModel(model, poll, true);
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Poll.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var poll = _pollService.GetPollById(id);
            if (poll != null)
            {
                _pollService.DeletePoll(poll);

                NotifySuccess(T("Admin.ContentManagement.Polls.Deleted"));
            }

            return RedirectToAction("List");
        }

        #endregion

        #region Poll answer

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Poll.Read)]
        public ActionResult PollAnswers(int pollId, GridCommand command)
        {
            var model = new GridModel<PollAnswerModel>();
            var poll = _pollService.GetPollById(pollId);
            var answers = poll.PollAnswers.OrderBy(x => x.DisplayOrder).ToList();

            model.Data = answers.Select(x =>
            {
                return new PollAnswerModel
                {
                    Id = x.Id,
                    PollId = x.PollId,
                    Name = x.Name,
                    NumberOfVotes = x.NumberOfVotes,
                    DisplayOrder1 = x.DisplayOrder
                };
            });

            model.Total = answers.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Poll.EditAnswer)]
        public ActionResult PollAnswerUpdate(PollAnswerModel model, GridCommand command)
        {
            var pollAnswer = _pollService.GetPollAnswerById(model.Id);

            if (!ModelState.IsValid)
            {
                var modelStateErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            pollAnswer.Name = model.Name;
            pollAnswer.DisplayOrder = model.DisplayOrder1;

            _pollService.UpdatePoll(pollAnswer.Poll);

            return PollAnswers(pollAnswer.PollId, command);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Poll.EditAnswer)]
        public ActionResult PollAnswerAdd(int pollId, PollAnswerModel model, GridCommand command)
        {
            if (!ModelState.IsValid)
            {
                var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            var poll = _pollService.GetPollById(pollId);

            poll.PollAnswers.Add(new PollAnswer
            {
                Name = model.Name,
                DisplayOrder = model.DisplayOrder1
            });

            _pollService.UpdatePoll(poll);

            return PollAnswers(pollId, command);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Poll.EditAnswer)]
        public ActionResult PollAnswerDelete(int id, GridCommand command)
        {
            var pollAnswer = _pollService.GetPollAnswerById(id);
            var pollId = pollAnswer.PollId;

            _pollService.DeletePollAnswer(pollAnswer);

            return PollAnswers(pollId, command);
        }

        #endregion

        #region Voting records

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Poll.Read)]
        public ActionResult VotingRecords(int pollId, GridCommand command)
        {
            var model = new GridModel<PollVotingRecordModel>();
            var guestString = T("Admin.Customers.Guest").Text;
            var votings = _pollService.GetVotingRecords(pollId, command.Page - 1, command.PageSize);

            model.Data = votings.Select(x =>
            {
                var votingModel = new PollVotingRecordModel
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    IsGuest = x.Customer.IsGuest(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                    AnswerName = x.PollAnswer.Name,
                    Username = x.Customer.Username,
                    FullName = x.Customer.GetFullName()
                };

                votingModel.Email = x.Customer.Email.HasValue() ? x.Customer.Email : (votingModel.IsGuest ? guestString : "".NaIfEmpty());

                return votingModel;
            });

            model.Total = votings.TotalCount;

            return new JsonResult { Data = model };
        }

        #endregion
    }
}
