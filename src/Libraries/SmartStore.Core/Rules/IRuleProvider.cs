using System.Collections.Generic;
using System.Linq;
using SmartStore.Rules.Domain;

namespace SmartStore.Rules
{
    public interface IRuleProvider : IRuleVisitor
    {
        RuleDescriptorCollection RuleDescriptors { get; }
    }

    public abstract class RuleProviderBase : IRuleProvider
    {
        private RuleDescriptorCollection _descriptors;

        protected RuleProviderBase(RuleScope scope)
        {
            Scope = scope;
        }

        public RuleScope Scope { get; protected set; }

        public abstract IRuleExpression VisitRule(RuleEntity rule);

        public abstract IRuleExpressionGroup VisitRuleSet(RuleSetEntity rule);

        protected virtual void ConvertRule(RuleEntity entity, RuleExpression expression)
        {
            Guard.NotNull(entity, nameof(entity));
            Guard.NotNull(expression, nameof(expression));

            var descriptor = RuleDescriptors.FindDescriptor(entity.RuleType);
            if (descriptor == null)
            {
                // A descriptor for this entity data does not exist. Allow deletion of it.
                descriptor = new InvalidRuleDescriptor(Scope)
                {
                    Name = entity.RuleType,
                    DisplayName = entity.RuleType
                };
            }
            else if (descriptor.Scope != Scope)
            {
                throw new SmartException($"Differing rule scope {descriptor.Scope}. Expected {Scope}.");
            }

            expression.Id = entity.Id;
            expression.RuleSetId = entity.RuleSetId;
            expression.Descriptor = descriptor;
            expression.Operator = entity.Operator;
            expression.RawValue = entity.Value;
            expression.Value = entity.Value.Convert(descriptor.RuleType.ClrType);
        }

        public virtual RuleDescriptorCollection RuleDescriptors
        {
            get
            {
                if (_descriptors == null)
                {
                    var descriptors = LoadDescriptors().Cast<RuleDescriptor>();
                    _descriptors = new RuleDescriptorCollection(descriptors);
                }

                return _descriptors;
            }
        }

        protected abstract IEnumerable<RuleDescriptor> LoadDescriptors();
    }
}
