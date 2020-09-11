using System;

namespace SmartStore.Web.Framework.UI.Choices
{
    public class GenericChoiceModel : ChoiceModel
    {
        private readonly Func<ChoiceModel, string> _controlIdFn;

        public GenericChoiceModel(Func<ChoiceModel, string> controlIdFn)
        {
            Guard.NotNull(controlIdFn, nameof(controlIdFn));

            _controlIdFn = controlIdFn;
        }

        public override string BuildControlId()
        {
            return _controlIdFn(this);
        }
    }

    public class GenericChoiceItemModel : ChoiceItemModel
    {
        private readonly Func<ChoiceItemModel, string> _itemLabelFn;

        public GenericChoiceItemModel(Func<ChoiceItemModel, string> itemLabelFn)
        {
            Guard.NotNull(itemLabelFn, nameof(itemLabelFn));

            _itemLabelFn = itemLabelFn;
        }

        public override string GetItemLabel()
        {
            return _itemLabelFn(this);
        }
    }
}
