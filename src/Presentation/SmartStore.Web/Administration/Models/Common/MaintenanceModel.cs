using System;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Common
{
    public class MaintenanceModel : ModelBase
    {
        public MaintenanceModel()
        {
            DeleteGuests = new DeleteGuestsModel();
            DeleteExportedFiles = new DeleteExportedFilesModel();
            DeleteImageCache = new DeleteImageCacheModel();
        }

        public DeleteGuestsModel DeleteGuests { get; set; }
        public DeleteExportedFilesModel DeleteExportedFiles { get; set; }
        public DeleteImageCacheModel DeleteImageCache { get; set; }

        [SmartResourceDisplayName("Admin.System.Maintenance.SqlQuery")]
        public string SqlQuery { get; set; }

        #region Nested classes

        public class DeleteGuestsModel : ModelBase
        {
            [SmartResourceDisplayName("Admin.System.Maintenance.DeleteGuests.StartDate")]
            public DateTime? StartDate { get; set; }

            [SmartResourceDisplayName("Admin.System.Maintenance.DeleteGuests.EndDate")]
            public DateTime? EndDate { get; set; }

            [SmartResourceDisplayName("Admin.System.Maintenance.DeleteGuests.OnlyWithoutShoppingCart")]
            public bool OnlyWithoutShoppingCart { get; set; }

            public int? NumberOfDeletedCustomers { get; set; }
        }

        public class DeleteExportedFilesModel : ModelBase
        {
            [SmartResourceDisplayName("Admin.System.Maintenance.DeleteExportedFiles.StartDate")]
            public DateTime? StartDate { get; set; }

            [SmartResourceDisplayName("Admin.System.Maintenance.DeleteExportedFiles.EndDate")]
            public DateTime? EndDate { get; set; }

            public int? NumberOfDeletedFiles { get; set; }
			public int? NumberOfDeletedFolders { get; set; }
        }

        public class DeleteImageCacheModel : ModelBase
        {
            [SmartResourceDisplayName("Admin.System.Maintenance.DeleteImageCache.FileCount")]
            public long FileCount { get; set; }

            [SmartResourceDisplayName("Admin.System.Maintenance.DeleteImageCache.TotalSize")]
            public string TotalSize { get; set; }
        }

        #endregion
    }
}
