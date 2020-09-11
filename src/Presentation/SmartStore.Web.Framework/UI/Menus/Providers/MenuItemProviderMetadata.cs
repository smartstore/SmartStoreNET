using System;
using SmartStore.Core.Domain.Cms;

namespace SmartStore.Web.Framework.UI
{
    /// <summary>
    /// Applies metadata to menu item provider types which implement <see cref="IMenuItemProvider"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MenuItemProviderAttribute : Attribute
    {
        public MenuItemProviderAttribute(string providerName)
        {
            Guard.NotEmpty(providerName, nameof(providerName));

            ProviderName = providerName;
        }

        /// <summary>
        /// Unique name of the provider.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Indicates that the provider appends multiple items to the tree.
        /// The corresponding <see cref="MenuItemRecord"/> cannot have child elements and certain properties such as title, short description etc. are ignored. 
        /// </summary>
        public bool AppendsMultipleItems { get; set; }
    }


    /// <summary>
    /// Represents menu item provider registration metadata.
    /// </summary>
    public class MenuItemProviderMetadata
    {
        public string ProviderName { get; set; }

        public bool AppendsMultipleItems { get; set; }
    }
}
