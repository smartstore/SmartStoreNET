using System;
using SmartStore.Core.Localization;

namespace SmartStore.Web.Framework.Localization
{
	public interface IText
	{
		LocalizedString Get(string key, params object[] args);
	}
}
