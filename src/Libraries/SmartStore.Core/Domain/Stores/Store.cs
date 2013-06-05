
namespace SmartStore.Core.Domain.Stores
{
	/// <summary>
	/// Represents a store
	/// </summary>
	public partial class Store : BaseEntity
	{
		/// <summary>
		/// Gets or sets the name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the comma separated list of possible HTTP_HOST values
		/// </summary>
		public string Hosts { get; set; }

		/// <summary>
		/// Gets or sets the display order
		/// </summary>
		public int DisplayOrder { get; set; }
	}
}
