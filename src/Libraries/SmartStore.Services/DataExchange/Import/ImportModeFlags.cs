using System;

namespace SmartStore.Services.DataExchange.Import
{
    
    [Flags]
    public enum ImportModeFlags
    {
        Insert = 1,
        Update = 2
    }

}
