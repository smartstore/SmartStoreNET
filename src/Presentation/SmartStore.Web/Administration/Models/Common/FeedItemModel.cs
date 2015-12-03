using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Common
{
	[Serializable]
	public class FeedItemModel : ModelBase
	{
		public string Title { get; set; }
		public string Summary { get; set; }
		public string Link { get; set; }
		public string PublishDate { get; set; }

		public bool IsError { get; set; }
	}
}