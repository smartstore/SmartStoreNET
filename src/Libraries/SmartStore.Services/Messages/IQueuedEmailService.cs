using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Services.Messages
{
	public partial interface IQueuedEmailService
    {
        /// <summary>
        /// Inserts a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>
        void InsertQueuedEmail(QueuedEmail queuedEmail);

        /// <summary>
        /// Updates a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>
        void UpdateQueuedEmail(QueuedEmail queuedEmail);

        /// <summary>
        /// Deleted a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>
        void DeleteQueuedEmail(QueuedEmail queuedEmail);

		/// <summary>
		/// Deletes all queued emails
		/// </summary>
		/// <returns>The count of deleted entries</returns>
		int DeleteAllQueuedEmails();

        /// <summary>
        /// Gets a queued email by identifier
        /// </summary>
        /// <param name="queuedEmailId">Queued email identifier</param>
        /// <returns>Queued email</returns>
        QueuedEmail GetQueuedEmailById(int queuedEmailId);

        /// <summary>
        /// Get queued emails by identifiers
        /// </summary>
        /// <param name="queuedEmailIds">queued email identifiers</param>
        /// <returns>Queued emails</returns>
        IList<QueuedEmail> GetQueuedEmailsByIds(int[] queuedEmailIds);

        /// <summary>
        /// Search queued emails
        /// </summary>
		/// <param name="query">An object containing the query criteria</param>
        /// <returns>Email item collection</returns>
		IPagedList<QueuedEmail> SearchEmails(SearchEmailsQuery query);

		/// <summary>
		/// Sends a queued email
		/// </summary>
		/// <param name="queuedEmail">Queued email</param>
		/// <returns>Whether the operation succeeded</returns>
		bool SendEmail(QueuedEmail queuedEmail);

		/// <summary>
		/// Gets a queued email attachment by identifier
		/// </summary>
		/// <param name="id">Queued email attachment identifier</param>
		/// <returns>Queued email attachment</returns>
		QueuedEmailAttachment GetQueuedEmailAttachmentById(int id);

		/// <summary>
		/// Deleted a queued email attachment
		/// </summary>
		/// <param name="attachment">Queued email attachment</param>
		void DeleteQueuedEmailAttachment(QueuedEmailAttachment attachment);

		/// <summary>
		/// Load binary data of a queued email attachment
		/// </summary>
		/// <param name="attachment">Queued email attachment</param>
		/// <returns>Binary data if <c>attachment.StorageLocation</c> is <c>EmailAttachmentStorageLocation.Blob</c>, otherwise <c>null</c></returns>
		byte[] LoadQueuedEmailAttachmentBinary(QueuedEmailAttachment attachment);
	}
}
