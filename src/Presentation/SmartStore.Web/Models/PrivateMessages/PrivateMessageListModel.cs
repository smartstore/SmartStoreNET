using System.Collections.Generic;
using SmartStore.Core;

namespace SmartStore.Web.Models.PrivateMessages
{
    public partial class PrivateMessageListModel : PagedListBase
    {
        public PrivateMessageListModel(IPageable pageable) : base(pageable)
        {
        }

        public IList<PrivateMessageModel> Messages { get; set; }
    }
}