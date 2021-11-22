using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.StrubeExport.Models
{
    /// <summary>
    /// Describes in which order marked Fields will be written to a String during CSV serialisation
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class FieldOrderAttribute  : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        private int index = 0;

        // This is a positional argument
        public FieldOrderAttribute(int index)
        {
            this.index = index;
        }

        // This is a named argument
        public int Index
        {
            get { return index; }
            set { this.index = value; }
        }

    }
}