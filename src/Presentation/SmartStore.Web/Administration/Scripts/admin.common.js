var currentModalId = "";
function closeModalWindow() {
    var modal = $('#' + currentModalId).data('modal');
    if (modal)
        modal.hide();
    return false;
}
function openModalWindow(modalId) {
    currentModalId = modalId;
    $('#' + modalId).data('modal').show();
}


// global Admin namespace
var Admin = {

	checkboxCheck: function (obj, checked) {
		if (checked)
			$(obj).attr('checked', 'checked');
		else
			$(obj).removeAttr('checked');
	},

	checkAllOverriddenStoreValue: function (obj) {
		$('input.multi-store-override-option').each(function (index, elem) {
			Admin.checkboxCheck(elem, obj.checked);
			Admin.checkOverriddenStoreValue(elem);
		});
	},

	checkOverriddenStoreValue: function (checkbox) {
		var parentSelector = $(checkbox).attr('data-parent-selector').toString(),
			parent = (parentSelector.length > 0 ? $(parentSelector) : $(checkbox).closest('.onoffswitch-container').parent()),
			checked = $(checkbox).is(':checked');

		parent.find(':input:not([type=hidden])').each(function (index, elem) {
			if ($(elem).is('select')) {
				$(elem).select2(checked ? 'enable' : 'disable');
			}
			else if (!$(elem).hasClass('multi-store-override-option')) {
				var tData = $(elem).data('tTextBox');

				if (tData != null) {
					if (checked)
						tData.enable();
					else
						tData.disable();
				}
				else {
					if (checked)
						$(elem).removeAttr('disabled');
					else
						$(elem).attr('disabled', 'disabled');
				}
			}
		});
	}
};


