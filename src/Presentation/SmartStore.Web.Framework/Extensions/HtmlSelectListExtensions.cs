using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework
{
	public static class HtmlSelectListExtensions
	{
		public static SelectList ToSelectList<TEnum>(this TEnum enumObj, bool markCurrentAsSelected = true) where TEnum : struct
		{
			if (!typeof(TEnum).IsEnum)
				throw new ArgumentException("An Enumeration type is required.", "enumObj");

			var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
			var workContext = EngineContext.Current.Resolve<IWorkContext>();

			var values = from TEnum enumValue in Enum.GetValues(typeof(TEnum))
						 select new { ID = Convert.ToInt32(enumValue), Name = enumValue.GetLocalizedEnum(localizationService, workContext) };

			object selectedValue = null;
			if (markCurrentAsSelected)
				selectedValue = Convert.ToInt32(enumObj);

			return new SelectList(values, "ID", "Name", selectedValue);
		}

		/// <summary>
		/// Get a list of all stores
		/// </summary>
		public static IList<SelectListItem> ToSelectListItems(this IEnumerable<Store> stores)
		{
			var lst = new List<SelectListItem>();

			foreach (var store in stores)
			{
				lst.Add(new SelectListItem
				{
					Text = store.Name,
					Value = store.Id.ToString()
				});
			}
			return lst;
		}

		public static void SelectValue(this List<SelectListItem> lst, string value, string defaultValue = null)
		{
			if (lst != null)
			{
				var itm = lst.FirstOrDefault(i => i.Value.IsCaseInsensitiveEqual(value));

				if (itm == null && defaultValue != null)
					itm = lst.FirstOrDefault(i => i.Value.IsCaseInsensitiveEqual(defaultValue));

				if (itm != null)
					itm.Selected = true;
			}
		}
	}
}
