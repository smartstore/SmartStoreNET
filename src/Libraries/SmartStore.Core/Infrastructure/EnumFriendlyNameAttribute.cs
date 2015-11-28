using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore
{

    /// <summary>
    /// Provides a friendly display name for an enumerated type value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class EnumFriendlyNameAttribute : Attribute
    {
        public EnumFriendlyNameAttribute(string friendlyName)
        {
            FriendlyName = friendlyName;
        }

        public string FriendlyName { get; private set; }
    }

    /// <summary>
    /// Provides a description for an enumerated type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class EnumDescriptionAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="EnumDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="description">The description to store in this attribute.
        /// </param>
        public EnumDescriptionAttribute(string description)
        {
            Description = description;
        }

        /// <summary>
        /// Gets the description stored in this attribute.
        /// </summary>
        /// <value>The description stored in the attribute.</value>
        public string Description { get; private set; }
    }

}
