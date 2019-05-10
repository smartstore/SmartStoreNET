using System.Collections.Generic;
using SmartStore.Core;

namespace SmartStore.Services.DataExchange.Import.Events
{
    /// <summary>
    /// An event that is fired after an import of a data batch.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to be imported.</typeparam>
    public class ImportBatchExecutedEvent<TEntity> where TEntity : BaseEntity
    {
        public ImportBatchExecutedEvent(ImportExecuteContext context, IEnumerable<ImportRow<TEntity>> batch)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(batch, nameof(batch));

            Context = context;
            Batch = batch;
        }

        /// <summary>
        /// Context of the import.
        /// </summary>
        public ImportExecuteContext Context { get; private set; }

        /// <summary>
        /// Current batch of import data.
        /// </summary>
        public IEnumerable<ImportRow<TEntity>> Batch { get; private set; }
    }
}
