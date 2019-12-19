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
            if (descriptor.Metadata != null && 
                descriptor.Metadata.TryGetValue("ValueTemplateName", out var val) && 
                val is string name && 
                name.HasValue())
            {
                return name;
            }

            string templateName;
            var type = descriptor.RuleType.ClrType;

            // TODO: get template name.
            if (type == typeof(bool) || type == typeof(bool?))
            {
                templateName = "Boolean";
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
