using SmartStore.Core.Domain.Forums;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Customer;

namespace SmartStore.Web.Models.Boards
{
    public partial class ForumTopicRowModel : EntityModelBase
    {
        public ForumTopicRowModel()
        {
            LastPost = new LastPostModel();
        }

        public string Subject { get; set; }
        public string SeName { get; set; }
        public int FirstPostId { get; set; }
        public int LastPostId { get; set; }
        public bool Published { get; set; }

        public ForumTopicType ForumTopicType { get; set; }
        public int NumPosts { get; set; }
        public int Views { get; set; }
        public int NumReplies { get; set; }

        public int PostsPageSize { get; set; }
        public int TotalPostPages => PostsPageSize != 0
                    ? (NumPosts / PostsPageSize) + 1
                    : 1;

        public int CustomerId { get; set; }
        public bool AllowViewingProfiles { get; set; }
        public string CustomerName { get; set; }
        public bool IsCustomerGuest { get; set; }

        public LastPostModel LastPost { get; set; }
        public CustomerAvatarModel Avatar { get; set; }

        public string AnchorTag => FirstPostId == 0 ? string.Empty : string.Concat("#", FirstPostId);
    }
}