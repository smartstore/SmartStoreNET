using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Core.Domain.Catalog
{
	/// <summary>
	/// Represents a specification attribute
	/// </summary>
	[DataContract]
	public partial class SpecificationAttribute : BaseEntity, ILocalizedEntity, ISearchAlias
	{
        private ICollection<SpecificationAttributeOption> _specificationAttributeOptions;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the specification attribute alias
		/// </summary>
		[DataMember]
		public string Alias { get; set; }

		/// <summary>
		/// Gets or sets the display order
		/// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets whether the specification attribute will be shown on the product page.
		/// </summary>
		[DataMember]
		public bool ShowOnProductPage { get; set; }

		/// <summary>
		/// Gets or sets whether the specification attribute can be filtered. Only effective in accordance with MegaSearchPlus plugin.
		/// </summary>
		[DataMember]
		[Index]
		public bool AllowFiltering { get; set; }

		/// <summary>
		/// Gets or sets the sorting of facets. Only effective in accordance with MegaSearchPlus plugin.
		/// </summary>
		[DataMember]
		public FacetSorting FacetSorting { get; set; }

		/// <summary>
		/// Gets or sets the facet template hint. Only effective in accordance with MegaSearchPlus plugin.
		/// </summary>
		[DataMember]
		public FacetTemplateHint FacetTemplateHint { get; set; }

        /// <summary>
        /// Specifies whether option names should be included in the search index. Only effective in accordance with MegaSearchPlus plugin.
        /// </summary>
        [DataMember]
        public bool IndexOptionNames { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute options
        /// </summary>
        [DataMember]
		public virtual ICollection<SpecificationAttributeOption> SpecificationAttributeOptions
        {
			get { return _specificationAttributeOptions ?? (_specificationAttributeOptions = new HashSet<SpecificationAttributeOption>()); }
            protected set { _specificationAttributeOptions = value; }
        }
    }
}
