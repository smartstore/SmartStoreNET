namespace SmartStore.Services.Cms.Blocks
{
	/// <summary>
	/// When implemented on <see cref="IBlock"/> types, makes the block bindable to
	/// product, category and manufacturer entities. The UI will display a 'Data binding'
	/// section which allows selection of an entity.
	/// </summary>
	/// <remarks>
	/// The handler for a bindable block type MUST implement <see cref="IBindableBlockHandler"/>.
	/// </remarks>
	public interface IBindableBlock : IBlock
    {
		/// <summary>
		/// The name of the bound entity, e.g. 'product'.
		/// </summary>
		string BindEntityName { get; set; }

		/// <summary>
		/// The id of the bound entity.
		/// </summary>
        int? BindEntityId { get; set; }
    }
}
