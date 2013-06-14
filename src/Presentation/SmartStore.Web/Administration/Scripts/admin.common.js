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
	var elements = $('#' + key + ', #' + key + ' input, #' + key + ' textarea, #' + key + ' select');
	if (!$(obj).is(':checked')) {
		elements.attr('disabled', true);
		//Telerik elements are enabled/disabled some other way
		var telerikElements = elements.data("tTextBox");
		if (telerikElements !== undefined) {
			telerikElements.disable();
		}
	}
	else {
		elements.removeAttr('disabled');
		//Telerik elements are enabled/disabled some other way
		var telerikElements = elements.data("tTextBox");
		if (telerikElements !== undefined) {
			telerikElements.enable();
		}
	};
}

// codehint: sm-add
// global Admin namespace
var Admin = { };


