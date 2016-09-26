using SmartStore.Core.Configuration;

namespace SmartStore.MegaMenu.Settings
{

    public class MegaMenuSettings : ISettings
    {
        public MegaMenuSettings()
        {
            ProductRotatorInterval = 4000;
            ProductRotatorDuration = 800;
            ProductRotatorCycle = true;
            MenuMinHeight = 250;
        }

        /// <summary>
        /// Specifies the interval after which the rotator scrolls to the next element
        /// </summary>
        public int ProductRotatorInterval { get; set; }

        /// <summary>
        /// Specifies the duration the rotator needs to scroll to the next element
        /// </summary>
        public int ProductRotatorDuration { get; set; }

        /// <summary>
        /// Specifies whether the product rotator scrolls back to the first element after reaching the last scrollable element
        /// </summary>
        public bool ProductRotatorCycle { get; set; }
        
        /// <summary>
        /// Specifies the min height of dropdown menus
        /// </summary>
        public int MenuMinHeight { get; set; }
    }
}