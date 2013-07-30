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


function setLocation(url) {
    window.location.href = url;
}

function OpenWindow(query, w, h, scroll) {
    var l = (screen.width - w) / 2;
    var t = (screen.height - h) / 2;

    // TODO: (MC) temp only. Global viewport is larger now.
    // But add this value to the callers later.
    h += 100;

    winprops = 'resizable=1, height=' + h + ',width=' + w + ',top=' + t + ',left=' + l + 'w';
    if (scroll) winprops += ',scrollbars=1';
    var f = window.open(query, "_blank", winprops);
}


// codehint: sm-add
// global Admin namespace
var Admin = {

	checkAllOverriddenStoreValue: function (obj) {
		$('input.multi-store-override-option').each(function (index, elem) {
			$(elem).attr('checked', obj.checked);
			Admin.checkOverriddenStoreValue(elem);
		});
	},

	checkOverriddenStoreValue: function (obj) {
		var parentSelector = $(obj).attr('data-parent-selector').toString(),
			parent = (parentSelector.length > 0 ? $(parentSelector) : $(obj).parent()),
			enable = $(obj).is(':checked');

		parent.find(':input:not([type=hidden])').each(function (index, elem) {
			var clss = $(elem).attr('class') || '';

			if ($(elem).is('select')) {
				$(elem).select2(enable ? 'enable' : 'disable');
			}
			else if (clss != 'multi-store-override-option') {
				var tData = $(elem).data('tTextBox');

				if (tData != null) {
					if (enable)
						tData.enable();
					else
						tData.disable();
				}
				else {
					if (enable)
						$(elem).removeAttr('disabled');
					else
						$(elem).attr('disabled', true);
				}
			}
		});
	}
};


