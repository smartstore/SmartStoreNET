using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Localization;
using SmartStore.Core.Search;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.Admin.Models.Settings
{
    public class ForumSearchSettingsModel : ModelBase
    {
        public ForumSearchSettingsModel()
        {
            ForumFacet = new CommonFacetSettingsModel();
            CustomerFacet = new CommonFacetSettingsModel();
            DateFacet = new CommonFacetSettingsModel();
        }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.SearchMode")]
        public SearchMode SearchMode { get; set; }
        public List<SelectListItem> AvailableSearchModes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.Forum.SearchFields")]
        public List<string> SearchFields { get; set; }
        public List<SelectListItem> AvailableSearchFields { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.DefaultSortOrder")]
        public ForumTopicSorting DefaultSortOrder { get; set; }
        public SelectList AvailableDefaultSortOrders { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.InstantSearchEnabled")]
        public bool InstantSearchEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.InstantSearchNumberOfHits")]
        public int InstantSearchNumberOfHits { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.InstantSearchTermMinLength")]
        public int InstantSearchTermMinLength { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.FilterMinHitCount")]
        public int FilterMinHitCount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.FilterMaxChoicesCount")]
        public int FilterMaxChoicesCount { get; set; }

        public CommonFacetSettingsModel ForumFacet { get; set; }
        public CommonFacetSettingsModel CustomerFacet { get; set; }
        public CommonFacetSettingsModel DateFacet { get; set; }
    }

    public class ForumSearchSettingValidator : SmartValidatorBase<ForumSearchSettingsModel>
    {
        public ForumSearchSettingValidator(Localizer T, Func<string, bool> addRule)
        {
            if (addRule("InstantSearchNumberOfHits"))
            {
                RuleFor(x => x.InstantSearchNumberOfHits)
                    .Must(x => x >= 1 && x <= SearchSettingValidator.MaxInstantSearchItems)
                    .When(x => x.InstantSearchEnabled)
                    .WithMessage(T("Admin.Validation.ValueRange").Text.FormatInvariant(1, SearchSettingValidator.MaxInstantSearchItems));
            }
        }
    }
}