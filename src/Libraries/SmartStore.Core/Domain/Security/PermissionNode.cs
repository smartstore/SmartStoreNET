using System;

namespace SmartStore.Core.Domain.Security
{
    public interface IPermissionNode/* : ILocalizedEntity*/
    {
        int PermissionRecordId { get; }
        string SystemName { get; }
        bool? Allow { get; }
        int Value { get; }
    }

    [Serializable]
    public class PermissionNode : IPermissionNode
    {
        public int PermissionRecordId { get; set; }
        public string SystemName { get; set; }
        public bool? Allow { get; set; }

        public int Value
        {
            get
            {
                if (Allow.HasValue)
                {
                    return Allow.Value ? 1 : 0;
                }

                return -1;
            }
        }
    }
}
