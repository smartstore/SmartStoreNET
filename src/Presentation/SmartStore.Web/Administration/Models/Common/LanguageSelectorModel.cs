using System.Collections.Generic;
using SmartStore.Admin.Models.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Common
{
    public class LanguageSelectorModel : ModelBase
    {
        public LanguageSelectorModel()
        {
            AvailableLanguages = new List<LanguageModel>();
        }

        public IList<LanguageModel> AvailableLanguages { get; set; }

        public LanguageModel CurrentLanguage { get; set; }
    }
}