using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Services.Filter;

namespace SmartStore.Web.Models.Filter
{
	public partial class ProductFilterModel : ModelBase
	{
		public FilterProductContext Context { get; set; }
	}	// class
}