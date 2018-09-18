using System;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Messages
{
	public class QueuedEmailListModel : ModelBase
    {
        public QueuedEmailListModel()
        {
            SearchLoadNotSent = true;
            SearchMaxSentTries = 10;
        }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.List.StartDate")]
        public DateTime? SearchStartDate { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.List.EndDate")]
        public DateTime? SearchEndDate { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.List.FromEmail")]
        [AllowHtml]
        public string SearchFromEmail { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.List.ToEmail")]
        [AllowHtml]
        public string SearchToEmail { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.List.LoadNotSent")]
        public bool SearchLoadNotSent { get; set; }

		[SmartResourceDisplayName("Admin.System.QueuedEmails.List.SendManually")]
		public bool? SearchSendManually { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.List.MaxSentTries")]
        public int SearchMaxSentTries { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.List.GoDirectlyToNumber")]
        public int? GoDirectlyToNumber { get; set; }
    }
}