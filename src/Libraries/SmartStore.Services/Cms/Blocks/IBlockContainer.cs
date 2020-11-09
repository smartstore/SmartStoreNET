using System;

namespace SmartStore.Services.Cms.Blocks
{
    /// <summary>
    /// Contains base data for a block instance.
    /// </summary>
    public interface IBlockContainer
    {
        /// <summary>
        /// The type of the block, e.g. 'html'.
        /// </summary>
        string BlockType { get; }

        /// <summary>
        /// The actual block instance.
        /// </summary>
        IBlock Block { get; }

        /// <summary>
        /// The block metadata.
        /// </summary>
        IBlockMetadata Metadata { get; }

        /// <summary>
        /// The block title / display name.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Whether the block's text content color should be inversed so
        /// that it gets readable on dark backgrounds.
        /// </summary>
        bool IsInversed { get; }

        /// <summary>
        /// The block html id.
        /// </summary>
        string HtmlId { get; }

        /// <summary>
        /// The block css class name(s).
        /// </summary>
        string CssClass { get; }

        /// <summary>
        /// The block css style expression.
        /// </summary>
        string CssStyle { get; }
    }
}
