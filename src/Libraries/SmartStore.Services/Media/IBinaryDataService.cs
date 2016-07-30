using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
	public partial interface IBinaryDataService
	{
		/// <summary>
		/// Delete binary data entity
		/// </summary>
		/// <param name="binaryData">Binary data entity</param>
		/// <param name="publishEvent">Whether to publish event</param>
		void DeleteBinaryData(BinaryData binaryData, bool publishEvent = true);

		/// <summary>
		/// Update binary data entity
		/// </summary>
		/// <param name="binaryData">Binary data entity</param>
		/// <param name="publishEvent">Whether to publish event</param>
		void UpdateBinaryData(BinaryData binaryData, bool publishEvent = true);

		/// <summary>
		/// Insert binary data entity
		/// </summary>
		/// <param name="binaryData">Binary data entity</param>
		/// <param name="publishEvent">Whether to publish event</param>
		void InsertBinaryData(BinaryData binaryData, bool publishEvent = true);

		/// <summary>
		/// Get binary data by identifier
		/// </summary>
		/// <param name="id">Binary data identifier</param>
		/// <returns>Binary data entity</returns>
		BinaryData GetBinaryDataById(int id);
	}
}
