using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore.Core.Configuration
{  
    /// <summary>
    /// Marker attribute. Indicates that the settings should
    /// be persisted as a JSON string rather than splitted
    /// into single properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
	[Obsolete]
    public class JsonPersistAttribute : Attribute
    {
    }
}
