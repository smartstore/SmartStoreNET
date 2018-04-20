using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Localization;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.Web.Models.Catalog
{
	public interface IQuantityInput
	{
		int EnteredQuantity { get; }
		int MinOrderAmount { get; }
		int MaxOrderAmount { get; }
        int QuantityStep { get; }
		LocalizedValue<string> QuantityUnitName { get; }
		List<SelectListItem> AllowedQuantities { get; }
        QuantityControlType QuantiyControlType { get; }
    }
}