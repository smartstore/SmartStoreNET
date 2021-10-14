using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

using SmartStore.Core;

namespace QTRADO.WMAddOn.Domain
{
    // After you've created your domain record:
    // switch the project map configuration from Debug to EFMigrations
    // open the nuget package manager console
    // Choose your project as the default project
    // add your initial migration by typing: add-migration Initial
    public class Grossist : BaseEntity
    {
        public Grossist()
        {
            GrossistGuid = new Guid();
        }
        public Guid GrossistGuid { get; set; }
        [Index("IX_Grossist_GrossoNr", IsUnique =true)]
        public string GrossoNr { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }
}