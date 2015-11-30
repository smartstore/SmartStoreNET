using System.Collections.Generic;

namespace SmartStore.Web.Framework.Localization
{
    public interface ILocalizedModel
    {
    }

    public interface ILocalizedModel<TLocalizedModel> : ILocalizedModel
    {
        IList<TLocalizedModel> Locales { get; set; }
    }
}
