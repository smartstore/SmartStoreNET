using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Rules;
using SmartStore.Rules.Domain;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Rules
{
    public class RuleSetModel : EntityModelBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public RuleScope Scope { get; set; }
        public bool IsSubGroup { get; set; }
        public LogicalRuleOperator LogicalOperator { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public DateTime? LastProcessedOnUtc { get; set; }
        public ICollection<RuleEntity> Rules { get; set; }

        public IRuleExpressionGroup ExpressionGroup { get; set; }
        public IEnumerable<RuleDescriptor> AvailableDescriptors { get; set; }
    }
}