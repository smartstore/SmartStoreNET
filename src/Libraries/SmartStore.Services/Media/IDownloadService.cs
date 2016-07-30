using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Media
{
	public partial interface IDownloadService
    {
        /// <summary>
        /// Gets a download
        /// </summary>
        /// <param name="downloadId">Download identifier</param>
        /// <returns>Download</returns>
        Download GetDownloadById(int downloadId);

		/// <summary>
		/// Gets downloads by identifiers
		/// </summary>
		/// <param name="downloadIds">Download identifiers</param>
		/// <returns>List of download entities</returns>
		IList<Download> GetDownloadsByIds(int[] downloadIds);

        /// <summary>
        /// Gets a download by GUID
        /// </summary>
        /// <param name="downloadGuid">Download GUID</param>
        /// <returns>Download</returns>
        Download GetDownloadByGuid(Guid downloadGuid);

        /// <summary>
        /// Deletes a download
        /// </summary>
        /// <param name="download">Download</param>
        void DeleteDownload(Download download);

        /// <summary>
        /// Inserts a download
        /// </summary>
        /// <param name="download">Download</param>
		/// <param name="downloadBinary">Download binary, can be <c>null</c></param>
        void InsertDownload(Download download, byte[] downloadBinary);

		/// <summary>
		/// Updates the download but leaves the binary data untouched
		/// </summary>
		/// <param name="download">Download</param>
		void UpdateDownload(Download download);

		/// <summary>
		/// Updates the download including the associated binary data
		/// </summary>
		/// <param name="download">Download</param>
		/// <param name="downloadBinary">New download binary data associated with the download entity</param>
		void UpdateDownload(Download download, byte[] downloadBinary);

        /// <summary>
        /// Gets a value indicating whether download is allowed
        /// </summary>
        /// <param name="orderItem">Order item to check</param>
        /// <returns>True if download is allowed; otherwise, false.</returns>
        bool IsDownloadAllowed(OrderItem orderItem);

        /// <summary>
        /// Gets a value indicating whether license download is allowed
        /// </summary>
        /// <param name="orderItem">Order item to check</param>
        /// <returns>True if license download is allowed; otherwise, false.</returns>
        bool IsLicenseDownloadAllowed(OrderItem orderItem);

		/// <summary>
		/// Load binary data of a download
		/// </summary>
		/// <param name="download">Download</param>
		/// <returns>Binary data</returns>
		byte[] LoadDownloadBinary(Download download);
	}
}
