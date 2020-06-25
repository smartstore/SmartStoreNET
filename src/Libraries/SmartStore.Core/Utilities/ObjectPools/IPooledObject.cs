using System;

namespace SmartStore.Utilities.ObjectPools
{
    public interface IPooledObject
    {
        bool Return();
    }
}
