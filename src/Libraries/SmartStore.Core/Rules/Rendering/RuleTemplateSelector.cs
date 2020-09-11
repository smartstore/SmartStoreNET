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
            var type = descriptor.RuleType;

            if (descriptor.SelectList != null || type == RuleType.IntArray || type == RuleType.FloatArray || type == RuleType.StringArray)
            {
                templateName = "Dropdown";
            }
            else if (type == RuleType.Boolean || type == RuleType.NullableBoolean)
            {
                templateName = "Boolean";
            }
            else if (type == RuleType.Int || type == RuleType.NullableInt)
            {
                templateName = "Int32";
            }
            else if (type == RuleType.Float || type == RuleType.NullableFloat)
            {
                templateName = "Float";
            }
            else if (type == RuleType.Money)
            {
                templateName = "Decimal";
            }
            else if (type == RuleType.DateTime || type == RuleType.NullableDateTime)
            {
                templateName = "DateTime";
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
