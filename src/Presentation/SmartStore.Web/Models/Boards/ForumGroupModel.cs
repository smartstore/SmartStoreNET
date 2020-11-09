using System.Collections.Generic;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Boards
{
    public partial class ForumGroupModel : EntityModelBase
    {
        public ForumGroupModel()
        {
            Forums = new List<ForumRowModel>();
        }

        public LocalizedValue<string> Name { get; set; }
        public LocalizedValue<string> Description { get; set; }
        public string SeName { get; set; }

        public IList<ForumRowModel> Forums { get; set; }
    }
}