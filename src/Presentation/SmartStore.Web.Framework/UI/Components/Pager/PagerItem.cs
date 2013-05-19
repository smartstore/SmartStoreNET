using System;

namespace SmartStore.Web.Framework.UI
{

    public enum PagerItemType
    {
        FirstPage,
        PreviousPage,
        Page,
        Text,
        NextPage,
        LastPage,
        Misc
    }

    public enum PagerItemState
    {
        Normal,
        Disabled,
        Selected
    }

    public class PagerItem
    {

        public PagerItem(string text)
            : this(text, String.Empty, PagerItemType.Page, PagerItemState.Normal)
        {
        }

        public PagerItem(string text, string url)
            : this(text, url, PagerItemType.Page, PagerItemState.Normal)
        {
        }

        public PagerItem(string text, string url, PagerItemType itemType)
            : this(text, url, itemType, PagerItemState.Normal)
        {
        }

        public PagerItem(string text, string url, PagerItemType itemType, PagerItemState state)
        {
            this.Text = text;
            this.Url = url;
            this.Type = itemType;
            this.State = state;
            this.ExtraData = String.Empty;
        }

        public string Text { get; set; }
        public string Url { get; set; }
        public PagerItemState State { get; set; }
        public PagerItemType Type { get; set; }
        public string ExtraData { get; set; }

        public bool IsNavButton
        {
            get
            {
                return (Type == PagerItemType.FirstPage || Type == PagerItemType.PreviousPage || Type == PagerItemType.NextPage || Type == PagerItemType.LastPage);
            }
        }

    }
}
