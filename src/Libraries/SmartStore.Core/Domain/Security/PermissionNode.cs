using System;

namespace SmartStore.Core.Domain.Security
{
    public interface IPermissionNode/* : ILocalizedEntity*/
    {
        bool? Allow { get; }
        string SystemName { get; }
    }

    [Serializable]
    public class PermissionNode : IPermissionNode
    {
        //public int Id { get; set; }
        public bool? Allow { get; set; }
        public string SystemName { get; set; }
    }
}
