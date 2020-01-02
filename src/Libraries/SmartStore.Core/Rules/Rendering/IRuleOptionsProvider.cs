namespace SmartStore.Rules
{
    /// <summary>
    /// Allows to provide custom rule options to be diplayed in a remote select list. <seealso cref="RemoteRuleValueSelectList"/>.
    /// Use named registration like RegisterType<MyProvider>().Named<IRuleValueProvider>("MyProviderName")
    /// where "MyProviderName" equals <see cref="RemoteRuleValueSelectList.DataProviderName"/>.
    /// </summary>
    public partial interface IRuleOptionsProvider
    {
        /// <summary>
        /// Gets options for a rule.
        /// </summary>
        /// <param name="reason">The reason for the request.</param>
        /// <param name="expression">Rule expression</param>
        /// <param name="pageIndex">Page index if provided options are paged.</param>
        /// <param name="searchTerm">Optional search term entered by user in select control.</param>
        /// <returns>Rule options result.</returns>
        RuleOptionsResult GetOptions(RuleOptionsReason reason, IRuleExpression expression, int pageIndex, string searchTerm);
    }
}
