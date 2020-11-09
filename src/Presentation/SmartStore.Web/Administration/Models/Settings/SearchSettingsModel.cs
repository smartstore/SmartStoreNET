using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Localization;
using SmartStore.Core.Search;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.Admin.Models.Settings
{
    public partial class SearchSettingsModel : ModelBase
    {
        public SearchSettingsModel()
        {
            CategoryFacet = new CommonFacetSettingsModel();
            BrandFacet = new CommonFacetSettingsModel();
            PriceFacet = new CommonFacetSettingsModel();
            RatingFacet = new CommonFacetSettingsModel();
            DeliveryTimeFacet = new CommonFacetSettingsModel();
            AvailabilityFacet = new CommonFacetSettingsModel();
            NewArrivalsFacet = new CommonFacetSettingsModel();
            ForumSearchSettings = new ForumSearchSettingsModel();
        }

        public string SearchFieldsNote { get; set; }
        public bool IsMegaSearchInstalled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.SearchMode")]
        public SearchMode SearchMode { get; set; }
        public List<SelectListItem> AvailableSearchModes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.SearchFields")]
        public List<string> SearchFields { get; set; }
        public List<SelectListItem> AvailableSearchFields { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.InstantSearchEnabled")]
        public bool InstantSearchEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.ShowProductImagesInInstantSearch")]
        public bool ShowProductImagesInInstantSearch { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.InstantSearchNumberOfHits")]
        public int InstantSearchNumberOfProducts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.InstantSearchTermMinLength")]
        public int InstantSearchTermMinLength { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.FilterMinHitCount")]
        public int FilterMinHitCount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.FilterMaxChoicesCount")]
        public int FilterMaxChoicesCount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.DefaultSortOrder")]
        public ProductSortingEnum DefaultSortOrder { get; set; }
        public SelectList AvailableSortOrderModes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.SearchProductByIdentificationNumber")]
        public bool SearchProductByIdentificationNumber { get; set; }

        public CommonFacetSettingsModel CategoryFacet { get; set; }
        public CommonFacetSettingsModel BrandFacet { get; set; }
        public CommonFacetSettingsModel PriceFacet { get; set; }
        public CommonFacetSettingsModel RatingFacet { get; set; }
        public CommonFacetSettingsModel DeliveryTimeFacet { get; set; }
        public CommonFacetSettingsModel AvailabilityFacet { get; set; }
        public CommonFacetSettingsModel NewArrivalsFacet { get; set; }

        // Property name must equal settings class name.
        public ForumSearchSettingsModel ForumSearchSettings { get; set; }
    }

    public class CommonFacetSettingsModel : ModelBase, ILocalizedModel<CommonFacetSettingsLocalizedModel>
    {
        public CommonFacetSettingsModel()
        {
            Locales = new List<CommonFacetSettingsLocalizedModel>();
        }

        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Common.Deactivated")]
        public bool Disabled { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.IncludeNotAvailable")]
        public bool IncludeNotAvailable { get; set; }

        public IList<CommonFacetSettingsLocalizedModel> Locales { get; set; }
    }

    public class CommonFacetSettingsLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Search.CommonFacet.Alias")]
        public string Alias { get; set; }
    }


    public class SearchSettingValidator : SmartValidatorBase<SearchSettingsModel>
    {
        public static int MaxInstantSearchItems => 16;

        public SearchSettingValidator(Localizer T, Func<string, bool> addRule)
        {
            if (addRule("InstantSearchNumberOfProducts"))
            {
                RuleFor(x => x.InstantSearchNumberOfProducts)
                    .Must(x => x >= 1 && x <= MaxInstantSearchItems)
                    .When(x => x.InstantSearchEnabled)
                    .WithMessage(T("Admin.Validation.ValueRange").Text.FormatInvariant(1, MaxInstantSearchItems));
            }
        }
    }
}