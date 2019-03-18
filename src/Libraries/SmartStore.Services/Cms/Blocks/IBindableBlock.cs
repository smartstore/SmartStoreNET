namespace SmartStore.Services.Cms.Blocks
{
    public interface IBindableBlock : IBlock
    {
        string BindEntityName { get; set; }
        int? BindEntityId { get; set; }
    }
}
