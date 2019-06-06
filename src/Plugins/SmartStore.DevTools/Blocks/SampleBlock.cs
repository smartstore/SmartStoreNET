using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using Newtonsoft.Json;
using SmartStore.Services.Cms.Blocks;
using SmartStore.Web.Framework;

namespace SmartStore.DevTools.Blocks
{
    /// <summary>
    /// The block handler is the controller which is responsible for loading, instantiating, storing and rendering block types.
    /// The 'BlockHandlerBase' abstract class already implements all relevant parts.
    /// You can, however, overwrite any method to fulfill your custom needs.
    /// </summary>
    [Block("sample", Icon = "far fa-terminal", FriendlyName = "Sample", DisplayOrder = 50, IsInternal = true)] // REMOVE IsInternal = true to display the block in the page builder
    public class SampleBlockHandler : BlockHandlerBase<SampleBlock>   
	{
        public override SampleBlock Load(IBlockEntity entity, StoryViewMode viewMode)
        {
            var block = base.Load(entity, viewMode);

            // If you're using your own data structure in your plugin this is where you would load data from your own data context

            if (viewMode == StoryViewMode.Edit)
            {
                // you can prepare your model especially for the edit mode of the block 
                // e.g. add some SelectListItems for dropdowns which you will only need in the edit mode of the block
                block.MyProperties.Add(new SelectListItem { Text = "Item1", Value = "1" });
            }
            else if (viewMode == StoryViewMode.GridEdit)
            {
                // Manipulate properties especially for the grid edit mode e.g. turn of any animation which could distract the user from editing the grid. 
                //block.Autoplay = false;
            }

            return block;
        }

        public override void Save(SampleBlock block, IBlockEntity entity)
        {
            // If you're using your own data structure in your plugin this is where you would save data to your own data context

            base.Save(block, entity);
        }

        /// <summary>
        /// By default block templates (Edit & Public) will be searched in '\Views\Story\BlockTemplates\BlockType' of your project (plugin) 
        /// The public action can address a deviating route by overwriting RenderCore & GetRoute
        /// </summary>
        //protected override void RenderCore(IBlockContainer element, IEnumerable<string> templates, HtmlHelper htmlHelper, TextWriter textWriter)
        //{
        //    if (templates.First() == "Edit")
        //    {
        //        base.RenderCore(element, templates, htmlHelper, textWriter);
        //    }
        //    else
        //    {
        //        base.RenderByChildAction(element, templates, htmlHelper, textWriter);
        //    }
        //}

        //protected override RouteInfo GetRoute(IBlockContainer element, string template)
        //{
        //    var block = (SampleBlock)element.Block;
        //    return new RouteInfo("BackendExtension", "DevTools", new
        //    {
        //        storyId = block.Property,
        //        area = "SmartStore.DevTools"
        //    });
        //}
    }

    /// <summary>
    /// Any type that implements the 'IBlock' interface acts as a model.
    /// </summary>
    [Validator(typeof(FrameBlockValidator))]
    public class SampleBlock : IBlock
	{
        [SmartResourceDisplayName("Plugins.SmartStore.DevTools.Block.Property")]
        public string Property { get; set; }

        // Per default a block instance will be converted to JSON and stored in the 'Model' field of the 'PageStoryBlock' table.
        // If your block type contains some special properties - e.g. volatile data for the edit mode - and you don't want them to be persisted, add the [JsonIgnore] attribute to your property.
        [JsonIgnore]
        public IList<SelectListItem> MyProperties { get; set; }
    }

    /// <summary>
    /// This is the validator which is used to validate the user input while editing your block
    /// </summary>
    public partial class FrameBlockValidator : AbstractValidator<SampleBlock>
    {
        public FrameBlockValidator()
        {
            RuleFor(x => x.Property).NotEmpty();
        }
    }
}