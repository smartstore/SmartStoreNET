using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Rules;
using SmartStore.Rules.Domain;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Rules
{
    public class RuleSetModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Rules.RuleSet.Fields.Name")]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Rules.RuleSet.Fields.Description")]
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.Rules.RuleSet.Fields.IsActive")]
        public bool IsActive { get; set; }

        [SmartResourceDisplayName("Admin.Rules.RuleSet.Fields.Scope")]
        public RuleScope Scope { get; set; }

        // TODO: show in grid
        public string ScopeName { get; set; }

        [SmartResourceDisplayName("Admin.Rules.RuleSet.Fields.IsSubGroup")]
        public bool IsSubGroup { get; set; }

        [SmartResourceDisplayName("Admin.Rules.RuleSet.Fields.LogicalOperator")]
        public LogicalRuleOperator LogicalOperator { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public DateTime? LastProcessedOnUtc { get; set; }
        public ICollection<RuleEntity> Rules { get; set; }

        public IRuleExpressionGroup ExpressionGroup { get; set; }
        public IEnumerable<RuleDescriptor> AvailableDescriptors { get; set; }
    }
}