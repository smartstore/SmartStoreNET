using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
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
		/// Get a select list of all stores
		/// </summary>
		public static IList<SelectListItem> ToSelectListItems(this IEnumerable<Store> stores, params int[] selectedStoreIds)
		{
			var list = new List<SelectListItem>();

			foreach (var store in stores)
			{
				list.Add(new SelectListItem
				{
					Text = store.Name,
					Value = store.Id.ToString(),
					Selected = selectedStoreIds != null && selectedStoreIds.Contains(store.Id)
				});
			}

			return list;
		}

		/// <summary>
		/// Get a select list of all customer roles
		/// </summary>
		public static IList<SelectListItem> ToSelectListItems(this IEnumerable<CustomerRole> roles, params int[] selectedCustomerRoleIds)
		{
			var list = new List<SelectListItem>();

			foreach (var role in roles)
			{
				list.Add(new SelectListItem
				{
					Text = role.Name,
					Value = role.Id.ToString(),
					Selected = selectedCustomerRoleIds != null && selectedCustomerRoleIds.Contains(role.Id)
				});
			}

			return list;
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
