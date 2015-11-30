using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Localization;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.Localization
{
	public class Text : IText
	{
		private readonly ILocalizationService _localizationService;

		public Text(ILocalizationService localizationService)
		{
			this._localizationService = localizationService;
		}
		
		public LocalizedString Get(string key, params object[] args)
		{
			var value = _localizationService.GetResource(key);
			if (string.IsNullOrEmpty(value))
			{
				return new LocalizedString(key);
			}
			return
				new LocalizedString((args == null || args.Length == 0)
										? value
										: string.Format(value, args), key, args);
		}
	}
}
