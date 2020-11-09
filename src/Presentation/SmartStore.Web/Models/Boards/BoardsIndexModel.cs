using System;
using System.Collections.Generic;

namespace SmartStore.Web.Models.Boards
{
    public partial class BoardsIndexModel
    {
        public BoardsIndexModel()
        {
            ForumGroups = new List<ForumGroupModel>();
        }

        public DateTime CurrentTime { get; set; }

        public IList<ForumGroupModel> ForumGroups { get; set; }

        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
    }
}