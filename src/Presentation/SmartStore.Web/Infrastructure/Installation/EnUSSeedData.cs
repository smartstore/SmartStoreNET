using System.Collections.Generic;
using SmartStore.Core.Configuration;
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
