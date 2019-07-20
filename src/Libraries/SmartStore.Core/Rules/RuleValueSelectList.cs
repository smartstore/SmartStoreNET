using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public abstract class RuleValueSelectList
    {
        public bool Multiple { get; set; }
    }

    public class RuleValueSelectListOption
    {
        public object Value { get; set; }
        public string Text { get; set; }
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

        public string DataSource { get; set; }
    }
}
