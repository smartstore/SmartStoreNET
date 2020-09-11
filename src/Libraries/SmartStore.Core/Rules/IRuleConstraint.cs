namespace SmartStore.Rules
{
    public interface IRuleConstraint
    {
        bool Match(RuleExpression expression);
    }
}
