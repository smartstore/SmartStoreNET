using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;
using SmartStore.Utilities;

namespace SmartStore.Templating.Liquid
{
	public static class TranslationFilters
	{
		public static string T(Context context, string key, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null)
		{
			var engine = (LiquidTemplateEngine)Template.FileSystem;
			int languageId = 0;

			if (context["Context.LanguageId"] is int lid)
			{
				languageId = lid;
			}

			var args = (new object[] { arg1, arg2, arg3, arg4 }).ToArray();

			return engine.T(key, languageId, args);
		}
	}
}
