using System;

namespace SmartStore
{
    public interface IOrdered
    {
        // TODO: (MC) Make Nullable!
        int Ordinal { get; }
    }
}
