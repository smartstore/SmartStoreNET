using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Media;
using SmartStore.Data.Setup;

namespace SmartStore.Web.Infrastructure.Installation
{

    public class EnUSSeedData : InvariantSeedData
    {

		public EnUSSeedData()
        {
        }

        protected override void Alter(IList<ISettings> settings)
        {
            base.Alter(settings);
        }
    }
}
