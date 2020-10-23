using System;
using System.Collections.Generic;
using System.IO;
using SmartStore.Collections;
using SmartStore.Core;
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
        /// Gets downloads assigned to an entity
        /// </summary>
        /// <param name="entity">Entity to get download for</param>
        /// <returns>List of download entities sorted by FileVersion</returns>
        IList<Download> GetDownloadsFor<TEntity>(TEntity entity, bool versionedFilesOnly = false) where TEntity : BaseEntity;

        /// <summary>
        /// Gets downloads by entity identifier
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="entityName">Entity name</param>
        /// <returns>List of download entities</returns>
        IList<Download> GetDownloadsFor(int entityId, string entityName, bool versionedFilesOnly = false);

        /// <summary>
		/// Gets downloads by entity identifier & fileversion
		/// </summary>
		/// <param name="entityId">Entity identifier</param>
		/// <param name="entityName">Entity name</param>
        /// <param name="fileVersion">File version</param>
		/// <returns>Download entity</returns>
        Download GetDownloadByVersion(int entityId, string entityName, string fileVersion);

        /// <summary>
        /// Gets downloads by entity identifiers
        /// </summary>
        /// <param name="entityIds">Entity identifiers</param>
        /// <param name="entityName">Entity name</param>
        /// <returns>Multimap of download entities</returns>
        Multimap<int, Download> GetDownloadsByEntityIds(int[] entityIds, string entityName);

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
        void InsertDownload(Download download);

        /// <summary>
        /// Inserts a download.
        /// </summary>
        /// <param name="download">Download entity.</param>
        /// <param name="stream">Data stream.</param>
        /// <param name="fileName">File name.</param>
        /// <returns>The inserted <see cref="MediaFileInfo"/> object instance.</returns>
        MediaFileInfo InsertDownload(Download download, Stream stream, string fileName);

        /// <summary>
        /// Updates the download.
        /// </summary>
        /// <param name="download">Download.</param>
        void UpdateDownload(Download download);

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
