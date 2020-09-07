using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Framework.Localization
{
	public interface ILocalizedModel<T> where T : ILocalizedModelLocal
    {
        IList<T> Locales { get; set; }
    }

	public interface ILocalizedModelLocal
	{
		int LanguageId { get; set; }
	}
}