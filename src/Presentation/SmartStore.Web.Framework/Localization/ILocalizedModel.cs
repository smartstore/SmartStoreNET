using System.Collections.Generic;

namespace SmartStore.Web.Framework.Localization
{
    public interface ILocalizedModel
    {
    }

    public interface ILocalizedModel<T> : ILocalizedModel where T : ILocalizedModelLocal
    {
        IList<T> Locales { get; set; }
    }

    public interface ILocalizedModelLocal
    {
        int LanguageId { get; set; }
    }
}