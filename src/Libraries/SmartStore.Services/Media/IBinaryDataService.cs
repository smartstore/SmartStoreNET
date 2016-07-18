using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
	public partial interface IBinaryDataService
	{
		/// <summary>
		/// Delete binary data entity
		/// </summary>
		/// <param name="binaryData">Binary data entity</param>
		void DeleteBinaryData(BinaryData binaryData);

		/// <summary>
		/// Update binary data entity
		/// </summary>
		/// <param name="binaryData">Binary data entity</param>
		void UpdateBinaryData(BinaryData binaryData);

		/// <summary>
		/// Insert binary data entity
		/// </summary>
		/// <param name="binaryData">Binary data entity</param>
		void InsertBinaryData(BinaryData binaryData);

		/// <summary>
		/// Get binary data by identifier
		/// </summary>
		/// <param name="id">Binary data identifier</param>
		/// <returns>Binary data entity</returns>
		BinaryData GetBinaryDataById(int id);
	}
}
