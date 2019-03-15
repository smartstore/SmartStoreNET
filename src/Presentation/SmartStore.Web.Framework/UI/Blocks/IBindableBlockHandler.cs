using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI.Blocks
{
    public interface IBindableBlockHandler : IBlockHandler
    {
        TemplateMappingConfiguration GetMappingConfiguration();
	}

    public class TemplateMappingConfiguration
    {
        public IList<TemplateMapping> Map { get; set; }
        public string[] BindableFields { get; set; }
    }

    public class TemplateMapping
    {
        public string ModelFieldName { get; set; }
        public string Template { get; set; }
    }
}
