using System;
using System.Collections.Generic;

namespace SmartStore.Rules
{
    public class RuleTemplateSelector : IRuleTemplateSelector
    {
        public RuleTemplateInfo GetTemplate(RuleDescriptor descriptor)
        {
            Guard.NotNull(descriptor, nameof(descriptor));

            var info = new RuleTemplateInfo
            {
                TemplateName = GetValueTemplateName(descriptor)
            };

            return info;
        }

        protected virtual string GetValueTemplateName(RuleDescriptor descriptor)
        {
            if (descriptor.Metadata.TryGetValue("ValueTemplateName", out var val) && 
                val is string name && 
                name.HasValue())
            {
                return name;
            }

            string templateName;
            var type = descriptor.RuleType.ClrType;

            if (type == typeof(bool) || type == typeof(bool?))
            {
                templateName = "Boolean";
            }
            else if (type == typeof(int) || type == typeof(int?))
            {
                templateName = "Int32";
            }
            else if (type == typeof(float) || type == typeof(float?))
            {
                templateName = "Float";
            }
            else if (type == typeof(decimal))
            {
                templateName = "Decimal";
            }
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                templateName = "DateTime";
            }
            else if (type == typeof(List<int>) || type == typeof(List<float>) || type == typeof(List<string>))
            {
                templateName = "Dropdown";
            }
            else
            {
                // Fallback to simple text-box.
                templateName = "TextBox";
            }

            return "ValueTemplates/" + templateName;
        }
    }
}
