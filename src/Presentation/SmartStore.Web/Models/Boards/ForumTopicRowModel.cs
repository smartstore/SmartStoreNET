using SmartStore.Core.Domain.Forums;

namespace SmartStore.Web.Models.Boards
{
    public partial class ForumTopicRowModel
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string SeName { get; set; }
        public int FirstPostId { get; set; }
        public int LastPostId { get; set; }

        public ForumTopicType ForumTopicType { get; set; }
        public int NumPosts { get; set; }
        public int Views { get; set; }
        public int NumReplies { get; set; }

        public int PostsPageSize { get; set; }
        public int TotalPostPages
        {
            get
            {
                return PostsPageSize != 0
                    ? (NumPosts / PostsPageSize) + 1
                    : 1;
            }
        }

        public int CustomerId { get; set; }
        public bool AllowViewingProfiles { get; set; }
        public string CustomerName { get; set; }
        public bool IsCustomerGuest { get; set; }

        public string AnchorTag
        {
            get
            {
                return FirstPostId == 0 ? string.Empty : string.Concat("#", FirstPostId);
            }
        }
    }
}