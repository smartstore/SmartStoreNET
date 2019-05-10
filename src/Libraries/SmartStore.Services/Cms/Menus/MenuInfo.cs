namespace SmartStore.Services.Cms
{
    public class MenuInfo
    {
        public int Id { get; set; }
        public string SystemName { get; set; }
        public string Template { get; set; }
        public string[] WidgetZones { get; set; }
        public int DisplayOrder { get; set; }
    }
}
