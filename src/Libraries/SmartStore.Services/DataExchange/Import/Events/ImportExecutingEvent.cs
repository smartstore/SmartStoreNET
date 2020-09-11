using System;

namespace SmartStore.Services.DataExchange.Import.Events
{
    public class ImportExecutingEvent
    {
        public ImportExecutingEvent(ImportExecuteContext context)
        {
            Guard.NotNull(context, nameof(context));

            Context = context;
        }

        public ImportExecuteContext Context
        {
            get;
            private set;
        }
    }
}
