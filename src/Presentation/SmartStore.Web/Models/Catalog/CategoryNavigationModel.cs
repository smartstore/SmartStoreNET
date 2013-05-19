using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Collections;

namespace SmartStore.Web.Models.Catalog
{
    public partial class CategoryNavigationModel : ModelBase
    {

        public int CurrentCategoryId { get; set; }
        public TreeNode<CategoryModel> Root { get; set; }
        public IList<int> Path { get; set; }

        public NodePathState GetNodePathState(TreeNode<CategoryModel> node)
        {
            var state = NodePathState.Unknown;

            if (node.HasChildren)
            {
                state = state | NodePathState.Parent;
            }

            if (node.Value.Id == this.CurrentCategoryId)
            {
                state = state | NodePathState.Selected;
            }
            else
            {
                if (node.Value.Level < this.Path.Count)
                {
                    if (this.Path[node.Value.Level] == node.Value.Id)
                    {
                        state = state | NodePathState.Expanded;
                    }
                }
            }

            return state;
        }

        public class CategoryModel : EntityModelBase, ICloneable<CategoryModel>
        {
            public string Name { get; set; }

            public string SeName { get; set; }

            // codehint: sm-edit (mc) - renamed, formerly 'NumberOfParentCategories'
            public int Level { get; set; }

            public int? NumberOfProducts { get; set; }

            public CategoryModel Clone()
            {
                return (CategoryModel)this.MemberwiseClone();
            }

            object ICloneable.Clone()
            {
                return this.Clone();
            }

            // codehint: sm-add
            public override string ToString()
            {
                return string.Format("{0} - {1}", this.Id, this.Name);
            }
        }

    }

    [Flags]
    public enum NodePathState
    {
        Unknown = 0,
        Parent = 1,
        Expanded = 2,
        Selected = 4
    }

}