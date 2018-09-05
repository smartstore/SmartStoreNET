using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class PagedListModel : ModelBase
    {
        public PagedListModel(IPageable pagedList)
        {
            Guard.NotNull(pagedList, nameof(pagedList));

            PagedList = pagedList;
            AvailablePageSizes = new int[0];
        }

        public IPageable PagedList { get; private set; }
        public IEnumerable<int> AvailablePageSizes { get; set; }
    }
}