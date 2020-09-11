using SmartStore.Rules.Domain;

namespace SmartStore.Rules
{
    public interface IRuleVisitor
    {
        RuleScope Scope { get; }
        IRuleExpression VisitRule(RuleEntity rule);
        IRuleExpressionGroup VisitRuleSet(RuleSetEntity ruleSet);
    }
}