using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Security;
using SmartStore.Services.Security;

namespace SmartStore.Web.Framework.WebApi.Security
{
    public partial class WebApiPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord AccessWebApi = new PermissionRecord { Name = "Access Web Api", SystemName = "AccessWebApi", Category = "Syndication" };
        
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[] 
            {
                AccessWebApi,
            };
        }

        public virtual IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();

            //uncomment code below in order to give appropriate permissions to admin by default
            //return new[] 
            //{
            //    new DefaultPermissionRecord 
            //    {
            //        CustomerRoleSystemName = SystemCustomerRoleNames.Administrators,
            //        PermissionRecords = new[] 
            //        {
            //            AccessWebService,
            //        }
            //    },
            //};
        }
    }
}