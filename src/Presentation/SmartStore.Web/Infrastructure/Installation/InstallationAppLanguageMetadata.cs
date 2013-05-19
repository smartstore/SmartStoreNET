using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.Web.Infrastructure.Installation
{
    public class InstallationAppLanguageMetadata
    {
        public string Culture { get; set; }
        public string Name { get; set; }
        public string UniqueSeoCode { get; set; }
        public string FlagImageFileName { get; set; }
    }
}