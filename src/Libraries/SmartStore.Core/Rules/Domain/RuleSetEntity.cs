using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core;

namespace SmartStore.Rules.Domain
{
    [Table("RuleSet")]
    public partial class RuleSetEntity // : BaseEntity
    {
        private ICollection<RuleEntity> _rules;

        // TODO: remove late
        public int Id { get; set; }

        [DataMember]
        [StringLength(200)]
        public string Name { get; set; }

        [DataMember]
        [StringLength(400)]
        public string Description { get; set; }

        [Index("IX_RuleSetEntity_Scope", Order = 0)]
        public bool IsActive { get; set; } = true;

        [Required]
        [Index("IX_RuleSetEntity_Scope", Order = 1)]
        public RuleScope Scope { get; set; }
        

        /// <summary>
        /// True when this set is an internal composite container for rules within another ruleset.
        /// </summary>
        public bool IsComposite { get; set; }

        /// <summary>
        /// Only applicable if <see cref="IsComposite"/> is true (???)
        /// </summary>
        public LogicalRuleOperator LogicalOperator { get; set; }

        public virtual ICollection<RuleEntity> Rules
        {
            get { return _rules ?? (_rules = new HashSet<RuleEntity>()); }
            protected internal set { _rules = value; }
        }
    }
}
