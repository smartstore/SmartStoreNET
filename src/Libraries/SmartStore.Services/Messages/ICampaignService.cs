using System.Collections.Generic;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Services.Messages
{
    public partial interface ICampaignService
    {
        /// <summary>
        /// Inserts a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>        
        void InsertCampaign(Campaign campaign);

        /// <summary>
        /// Updates a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>
        void UpdateCampaign(Campaign campaign);

        /// <summary>
        /// Deleted a queued email
        /// </summary>
        /// <param name="campaign">Campaign</param>
        void DeleteCampaign(Campaign campaign);

        /// <summary>
        /// Gets a campaign by identifier
        /// </summary>
        /// <param name="campaignId">Campaign identifier</param>
        /// <returns>Campaign</returns>
        Campaign GetCampaignById(int campaignId);

        /// <summary>
        /// Gets all campaigns
        /// </summary>
        /// <returns>Campaign collection</returns>
        IList<Campaign> GetAllCampaigns();
        
        /// <summary>
        /// Sends a campaign to specified emails
        /// </summary>
        /// <param name="campaign">Campaign</param>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Total emails sent</returns>
        int SendCampaign(Campaign campaign, IEnumerable<NewsLetterSubscription> subscriptions);

		/// <summary>
		/// Creates a campaign email without sending it for previewing and testing purposes.
		/// </summary>
		/// <param name="campaign">The campaign to preview</param>
		/// <returns>The preview result</returns>
		CreateMessageResult Preview(Campaign campaign);
	}
}
