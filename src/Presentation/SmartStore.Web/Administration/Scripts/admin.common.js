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

function checkAllOverridenStoreValue(item) {
	$('.multi-store-override-option').each(function (k, v) {
		$(v).attr('checked', item.checked);
		checkOverridenStoreValue(v, $(v).attr('data-for-input-id'));
	});
}

function checkOverridenStoreValue(obj, key) {
	if (!$(obj).is(':checked')) {
		$('#' + key).attr('disabled', true);
		//Telerik elements are enabled/disabled some other way
		var telerikElement = $('#' + key).data("tTextBox");
		if (telerikElement !== undefined) {
			telerikElement.disable();
		}
	}
	else {
		$('#' + key).removeAttr('disabled');
		//Telerik elements are enabled/disabled some other way
		var telerikElement = $('#' + key).data("tTextBox");
		if (telerikElement !== undefined) {
			telerikElement.enable();
		}
	};
}

// codehint: sm-add
// global Admin namespace
var Admin = { };


