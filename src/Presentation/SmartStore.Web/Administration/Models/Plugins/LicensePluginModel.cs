using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Plugins
{
    public class LicensePluginModel : ModelBase
    {
        public string SystemName { get; set; }
        public int InvalidDataStoreId { get; set; }

        public List<StoreLicenseModel> StoreLicenses { get; set; }

        public class StoreLicenseModel
        {
            public StoreLicenseModel()
            {
                LicenseLabel = new LicenseLabelModel();
            }

            [SmartResourceDisplayName("Admin.Configuration.Plugins.LicenseKey")]
            public string LicenseKey { get; set; }

            public LicenseLabelModel LicenseLabel { get; set; }

            public int StoreId { get; set; }
            public string StoreName { get; set; }
            public string StoreUrl { get; set; }
        }
    }
}