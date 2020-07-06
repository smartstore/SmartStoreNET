using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Rules
{
    public abstract class RuleValueSelectList
    {
        public bool Multiple { get; set; }
        public bool Tags { get; set; }
    }

    public class RuleValueSelectListOption
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public string Hint { get; set; }
        public RuleValueSelectListGroup Group { get; set; }
    }

    public class RuleValueSelectListGroup
    {
        public string Name { get; set; }
        public int Order { get; set; }
    }

    public class LocalRuleValueSelectList : RuleValueSelectList
    {
        public LocalRuleValueSelectList() : this(null)
        {
        }

        public LocalRuleValueSelectList(params RuleValueSelectListOption[] options)
        {
            Options = new List<RuleValueSelectListOption>(options ?? Enumerable.Empty<RuleValueSelectListOption>());
        }

        public IEnumerable<RuleValueSelectListOption> Options { get; protected set; }
    }

    public class RemoteRuleValueSelectList : RuleValueSelectList
    {
        public RemoteRuleValueSelectList(string dataSource)
        {
            Guard.NotEmpty(dataSource, nameof(dataSource));

            DataSource = dataSource;
        }

        /// <summary>
        /// Name of the data source.
        /// </summary>
        public string DataSource { get; protected set; }
    }
}
