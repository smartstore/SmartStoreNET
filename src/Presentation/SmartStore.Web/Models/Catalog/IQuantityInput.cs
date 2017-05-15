using SmartStore.Core.Domain.Catalog;
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
        string QuantityUnitName { get; }
		List<SelectListItem> AllowedQuantities { get; }
        QuantityControlType QuantiyControlType { get; }
    }
}