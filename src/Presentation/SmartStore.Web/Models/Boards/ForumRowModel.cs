
using SmartStore.Services.Localization;

namespace SmartStore.Web.Models.Boards
{
    public partial class ForumRowModel
    {
        public int Id { get; set; }
        public LocalizedValue<string> Name { get; set; }
        public LocalizedValue<string> Description { get; set; }
		public string SeName { get; set; }
		public int NumTopics { get; set; }
        public int NumPosts { get; set; }
        public int LastPostId { get; set; }
    }
}