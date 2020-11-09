using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Polls
{
    [Validator(typeof(PollValidator))]
    public class PollModel : EntityModelBase
    {
        public bool UsernamesEnabled { get; set; }
        public int GridPageSize { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Polls.Fields.Language")]
        public int LanguageId { get; set; }
        public List<SelectListItem> AvailableLanguages { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Polls.Fields.Language")]
        [AllowHtml]
        public string LanguageName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Polls.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Polls.Fields.SystemKeyword")]
        [AllowHtml]
        public string SystemKeyword { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Polls.Fields.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Polls.Fields.ShowOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Polls.Fields.AllowGuestsToVote")]
        public bool AllowGuestsToVote { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Polls.Fields.StartDate")]
        public DateTime? StartDate { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Polls.Fields.EndDate")]
        public DateTime? EndDate { get; set; }

        // Store mapping
        [UIHint("Stores")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }
    }

    public partial class PollValidator : AbstractValidator<PollModel>
    {
        public PollValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}