using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Boards
{
    public partial class ForumRowModel : EntityModelBase
    {
        public ForumRowModel()
        {
            LastPost = new LastPostModel();
        }

        public LocalizedValue<string> Name { get; set; }
        public LocalizedValue<string> Description { get; set; }
        public string SeName { get; set; }
        public int NumTopics { get; set; }
        public int NumPosts { get; set; }
        public int LastPostId { get; set; }

        public LastPostModel LastPost { get; set; }
    }
}