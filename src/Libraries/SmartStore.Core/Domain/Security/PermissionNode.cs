using System;

namespace SmartStore.Core.Domain.Security
{
    public interface IPermissionNode/* : ILocalizedEntity*/
    {
        int PermissionRecordId { get; }
        bool? Allow { get; }
        string SystemName { get; }
    }

    [Serializable]
    public class PermissionNode : IPermissionNode
    {
        public int PermissionRecordId { get; set; }
        public bool? Allow { get; set; }
        public string SystemName { get; set; }
    }
}
