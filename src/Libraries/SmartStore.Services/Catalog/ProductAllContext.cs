using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Catalog
{
	public class ProductAllContext
	{
		/// <summary>
		/// Filter by product identifiers
		/// </summary>
		public List<int> ProductIds { get; set; }

		/// <summary>
		/// Filter by category identifiers
		/// </summary>
		public List<int> CategoryIds { get; set; }

		/// <summary>
		/// Filter by product's feature flag
		/// </summary>
		public bool? IncludeFeatured { get; set; }

		/// <summary>
		/// Filter by store identifier
		/// </summary>
		public int StoreId { get; set; }

		/// <summary>
		/// Filter by individually visible products
		/// </summary>
		public bool? VisibleIndividually { get; set; }

		/// <summary>
		/// Filter by availability date
		/// </summary>
		public bool FilterByAvailableDate { get; set; }
	}
}
