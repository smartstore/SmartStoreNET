using QTRADO.WMAddOn.Domain;

namespace QTRADO.WMAddOn.Services
{
    public partial interface IWMAddOnService
    {
        Grossist GetWMAddOnRecord(int entityId, string entityName);
        Grossist GetWMAddOnRecordById(int id);
        void InsertWMAddOnRecord(Grossist record);
        void UpdateWMAddOnRecord(Grossist record);
        void DeleteWMAddOnRecord(Grossist record);
    }
}
