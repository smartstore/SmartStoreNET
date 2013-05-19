using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore.Services
{
    
    public class DataImportResult
    {

        public DataImportResult()
        {
            this.Warnings = new List<string>();
        }

        public int TotalRecords { get; internal set; }
        public int AffectedRecords { get; internal set; }
        public int NewRecords { get; internal set; }
        public int ModifiedRecords { get; internal set; }
        public IList<string> Warnings { get; private set; }

        //public bool Success { get; internal set; }
        //public Exception Error { get; internal set; }

    }

}
