//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using SmartStore.Core.Plugins;

//namespace SmartStore.Rules.Cart
//{
//    /// <summary>
//    /// Applies metadata to rule evaluators which implement <see cref="IRule"/>.
//    /// </summary>
//    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
//    public sealed class RuleAttribute : Attribute
//    {
//        public RuleAttribute(string systemName)
//        {
//            Guard.NotNull(systemName, nameof(systemName));

//            SystemName = systemName;
//        }

//        /// <summary>
//        /// The rule system name
//        /// </summary>
//        public string SystemName { get; set; }

//        /// <summary>
//        /// The english friendly name of the rule
//        /// </summary>
//        public string FriendlyName { get; set; }

//        /// <summary>
//        /// The order of display
//        /// </summary>
//        public int DisplayOrder { get; set; }

//        public RuleScope Scope { get; set; }
//    }

//    /// <summary>
//    /// Represents rule registration metadata
//    /// </summary>
//    public interface IRuleMetadata : IProviderMetadata
//    {
//        RuleScope Scope { get; }
//    }

//    public class RuleMetadata : IRuleMetadata, ICloneable<RuleMetadata>
//    {
//        public string SystemName { get; set; }
//        public string FriendlyName { get; set; }
//        public string Description { get; set; }
//        public string ResourceKeyPattern { get; set; }
//        public int DisplayOrder { get; set; }

//        public RuleScope Scope { get; set; }

//        public RuleMetadata Clone() => (RuleMetadata)this.MemberwiseClone();
//        object ICloneable.Clone() => this.MemberwiseClone();
//    }
//}
