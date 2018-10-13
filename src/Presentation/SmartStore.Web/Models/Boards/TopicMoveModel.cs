using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Boards
{
    public partial class TopicMoveModel : EntityModelBase
    {
        public string TopicSeName { get; set; }
        public bool IsCustomerAllowedToEdit { get; set; }
        public int CustomerId { get; set; }

        public int ForumSelected { get; set; }
        public IList<SelectListItem> Forums { get; set; }
    }
}