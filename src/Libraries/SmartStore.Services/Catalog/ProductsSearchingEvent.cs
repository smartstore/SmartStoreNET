using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Services.Catalog
{
    public class ProductsSearchingEvent
    {
        public ProductsSearchingEvent(ProductSearchContext ctx)
        {
            SearchContext = ctx;
        }

        public ProductSearchContext SearchContext { get; private set; }
    }
}
