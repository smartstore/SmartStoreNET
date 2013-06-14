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

function checkAllOverriddenStoreValue(obj) {
	$('input.multi-store-override-option').each(function (k, v) {
		$(v).attr('checked', obj.checked);
		checkOverridenStoreValue(v);
	});
}
function checkOverriddenStoreValue(obj) {
	var parentSelector = $(obj).attr('data-parent-selector').toString(),
		parent = (parentSelector.length > 0 ? $(parentSelector) : $(obj).parent()),
		inputs = parent.find(':input').not('.multi-store-override-option');

	if ($(obj).is(':checked')) {
		inputs.removeAttr('disabled');
	}
	else {
		inputs.attr('disabled', true);
	}
}

// codehint: sm-add
// global Admin namespace
var Admin = { };


