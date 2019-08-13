using System;

namespace SmartStore.Core.Domain.Security
{
    public interface IPermissionNode
    {
        int PermissionRecordId { get; }
        string SystemName { get; }
        string DisplayName { get; }
        bool? Allow { get; }
    }

    [Serializable]
    public class PermissionNode : IPermissionNode
    {
        public int PermissionRecordId { get; set; }
        public string SystemName { get; set; }
        public string DisplayName { get; set; }
        public bool? Allow { get; set; }
    }
}
