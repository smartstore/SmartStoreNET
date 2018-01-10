using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core.Themes
{   
    /// <summary>
    /// Represents deserialized metadata for a theme variable
    /// </summary>
    public class ThemeVariableInfo : DisposableObject
    {
        /// <summary>
        /// Gets the variable name as specified in the config file
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the default variable value as specified in the config file
        /// </summary>
        public string DefaultValue { get; internal set; }

        /// <summary>
        /// Gets the variable type as specified in the config file
        /// </summary>
        public ThemeVariableType Type { get; internal set; }

        public string TypeAsString
        {
            get
            {
                if (this.Type != ThemeVariableType.Select)
                {
                    return this.Type.ToString();
                }

                return "Select#" + this.SelectRef;
            }
        }

        /// <summary>
        /// Gets the id of the select element or <c>null</c>,
        /// if the variable is not a select type.
        /// </summary>
        public string SelectRef { get; internal set; }

        /// <summary>
        /// Gets the theme manifest the variable belongs to
        /// </summary>
        public ThemeManifest Manifest { get; internal set; }


        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                this.Manifest = null;
            }
        }
    }
}
