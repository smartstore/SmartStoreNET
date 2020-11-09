using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;

namespace SmartStore.Data.Caching
{
    internal class CachingCommandDefinition : DbCommandDefinition
    {
        private readonly DbCommandDefinition _commandDefintion;
        private readonly CommandTreeFacts _commandTreeFacts;
        private readonly CacheTransactionInterceptor _cacheTransactionInterceptor;
        private readonly DbCachingPolicy _policy;

        public CachingCommandDefinition(
            DbCommandDefinition commandDefinition,
            CommandTreeFacts commandTreeFacts,
            CacheTransactionInterceptor cacheTransactionInterceptor,
            DbCachingPolicy policy)
        {
            _commandDefintion = commandDefinition;
            _commandTreeFacts = commandTreeFacts;
            _cacheTransactionInterceptor = cacheTransactionInterceptor;
            _policy = policy;
        }

        public bool IsQuery => _commandTreeFacts.IsQuery;

        public bool IsCacheable => _commandTreeFacts.IsQuery && !_commandTreeFacts.UsesNonDeterministicFunctions;

        public ReadOnlyCollection<EntitySetBase> AffectedEntitySets => _commandTreeFacts.AffectedEntitySets;

        public override DbCommand CreateCommand()
        {
            return new CachingCommand(_commandDefintion.CreateCommand(), _commandTreeFacts, _cacheTransactionInterceptor, _policy);
        }
    }
}
