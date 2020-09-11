using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Boards
{
    public partial class LastPostModel : EntityModelBase
    {
        public int ForumTopicId { get; set; }
        public string ForumTopicSeName { get; set; }
        public string ForumTopicSubject { get; set; }

        public int CustomerId { get; set; }
        public bool AllowViewingProfiles { get; set; }
        public string CustomerName { get; set; }
        public bool IsCustomerGuest { get; set; }

        public string PostCreatedOnStr { get; set; }
        public bool Published { get; set; }

        public bool ShowTopic { get; set; }
    }
}