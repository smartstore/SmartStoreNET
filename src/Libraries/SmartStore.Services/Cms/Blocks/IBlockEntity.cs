using System;

namespace SmartStore.Services.Cms.Blocks
{
    /// <summary>
    /// Represents a block instance's storage data.
    /// </summary>
    public interface IBlockEntity
    {
        int Id { get; set; }
        int StoryId { get; }
        string BlockType { get; set; }
        string Model { get; set; }
        string TagLine { get; set; }
        string Title { get; set; }
        string SubTitle { get; set; }
        string Body { get; set; }
        string Template { get; set; }
        string Custom1 { get; set; }
        string Custom2 { get; set; }
        string Custom3 { get; set; }
        string Custom4 { get; set; }
        string Custom5 { get; set; }

        string BindEntityName { get; set; }
        int? BindEntityId { get; set; }
    }
}
