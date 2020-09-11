using System;
using System.Collections.Generic;
using System.Threading;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange.Import
{
    public interface IDataImporter
    {
        void Import(DataImportRequest request, CancellationToken cancellationToken);
    }


    public class DataImportRequest
    {
        private readonly static ProgressValueSetter _voidProgressValueSetter = DataImportRequest.SetProgress;

        public DataImportRequest(ImportProfile profile)
        {
            Guard.NotNull(profile, nameof(profile));

            Profile = profile;
            ProgressValueSetter = _voidProgressValueSetter;

            EntitiesToImport = new List<int>();
            CustomData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public ImportProfile Profile { get; private set; }

        public ProgressValueSetter ProgressValueSetter { get; set; }

        public bool HasPermission { get; set; }

        public IList<int> EntitiesToImport { get; set; }

        public IDictionary<string, object> CustomData { get; private set; }


        private static void SetProgress(int val, int max, string msg)
        {
            // do nothing
        }
    }
}
