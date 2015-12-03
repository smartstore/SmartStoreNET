using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Admin.Models.Plugins
{

	public class ProviderModelList
	{
		public ProviderModelList()
		{
			this.ExtraColumns = new List<Func<dynamic, object>>();
		}

		public IEnumerable<ProviderModel> Data { get; set; }

		public IList<Func<dynamic, object>> ExtraColumns { get; set; }
	}
	
	public class ProviderModelList<TModel> : ProviderModelList where TModel : ProviderModel
	{

		public void SetData(IEnumerable<TModel> data)
		{
			base.Data = data;
		}

		public void SetColumns(IEnumerable<Func<TModel, object>> columns)
		{
			foreach (var col in columns)
			{
				Func<dynamic, object> fn = (x) => col(x);
				base.ExtraColumns.Add(fn);
			}
		}
	}
}