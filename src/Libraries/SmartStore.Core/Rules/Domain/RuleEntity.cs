using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SmartStore.Core;

namespace SmartStore.Rules.Domain
{
    public partial class RuleEntity : BaseEntity
    {
        [Required]
        public int RuleSetId { get; set; }

        [ForeignKey("RuleSetId")]
        [JsonIgnore]
        public virtual RuleSetEntity RuleSet { get; set; }

        [DataMember]
        [Required, StringLength(100)]
        [Index("IX_PageBuilder_RuleType")]
        public string RuleType { get; set; }

        //[DataMember]
        //[StringLength(400)]
        //public string Member { get; set; }

        [DataMember]
        [Required, StringLength(20)]
        public string Operator { get; set; }

        [DataMember]
        [MaxLength]
        public string Value { get; set; }

        [DataMember]
        [Index("IX_PageBuilder_DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NotMapped]
        public bool IsGroup => RuleType.IsCaseInsensitiveEqual("Group");
    }
}