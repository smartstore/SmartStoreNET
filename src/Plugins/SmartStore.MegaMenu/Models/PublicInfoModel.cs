using SmartStore.Collections;
using SmartStore.MegaMenu.Settings;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.UI;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.MegaMenu.Models
{
    public class MegaMenuNavigationModel
    {
        public TreeNode<MenuItem> Root { get; set; }
        public IList<MenuItem> Path { get; set; }

        public MenuItem SelectedMenuItem
        {
            get
            {
                if (Path == null || Path.Count == 0)
                    return null;

                return Path.Last();
            }
        }

        public MegaMenuSettings Settings { get; set; }
    }
}