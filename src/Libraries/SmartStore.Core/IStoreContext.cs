using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core
{
	/// <summary>
	/// Store context
	/// </summary>
	public interface IStoreContext
	{
		/// <summary>
		/// Sets a store override to be used for the current request
		/// </summary>
		/// <param name="storeId">The store override or <c>null</c> to remove the override</param>
		void SetRequestStore(int? storeId);

		/// <summary>
		/// Sets a store override to be used for the current user's session (e.g. for preview mode)
		/// </summary>
		/// <param name="storeId">The store override or <c>null</c> to remove the override</param>
		void SetPreviewStore(int? storeId);

		/// <summary>
		/// Gets the store override for the current request
		/// </summary>
		/// <returns>The store override or <c>null</c></returns>
		int? GetRequestStore();

		/// <summary>
		/// Gets the store override for the current session
		/// </summary>
		/// <returns>The store override or <c>null</c></returns>
		int? GetPreviewStore();
		
		/// <summary>
		/// Gets or sets the current store
		/// </summary>
		/// <remarks>Setter is for virtualization and testing purposes</remarks>
		Store CurrentStore { get; set; }

		/// <summary>
		/// IsSingleStoreMode ? 0 : CurrentStore.Id
		/// </summary>
		int CurrentStoreIdIfMultiStoreMode { get; }
	}
}
