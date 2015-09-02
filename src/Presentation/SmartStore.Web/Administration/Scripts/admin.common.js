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
	},

	TaskWatcher: (function () {
		var interval;

		return {
			startWatching: function (opts) {
				function poll() {
					$.ajax({
						cache: false,
						type: 'POST',
						global: false,
						url: opts.pollUrl,
						dataType: 'json',
						success: function (data) {
							data = data || [];
							var runningElements = [];
							$.each(data, function (i, task) {
								var el = $(opts.elementsSelector + '[data-task-id=' + task.id + ']');
								if (el.length) {
									runningElements.push(el[0]);
									if (el.data('running') && el.text()) {
										// already running
										el.find('.text').text(task.message || opts.defaultProgressMessage);
										el.find('.percent').text(task.percent >> 0 ? task.percent + ' %' : "");
									}
									else {
										// new task
										var row1 = $('<div class="hint clearfix" style="position: relative"></div>').appendTo(el);
										row1.append($('<div class="text pull-left">' + (task.message || opts.defaultProgressMessage) + '</div>'));
										row1.append($('<div class="percent pull-right">' + (task.percent >> 0 ? task.percent + ' %' : "") + '</div>'));
										var row2 = $('<div class="loading-bar" style="margin-top: 4px"></div>').appendTo(el);
										el.attr('data-running', 'true').data('running', true);
										if (_.isFunction(opts.onTaskStarted)) {
											opts.onTaskStarted(task, el);
										}
										el.removeClass('hide');
									}
								}
							});

							// remove runningElements for finished tasks (the ones currently running but are not in 'runningElements'
							var currentlyRunningElements = opts.context.find(opts.elementsSelector + '[data-running=true]');
							$.each(currentlyRunningElements, function (i, el) {
								var shouldRun = _.find(runningElements, function (val) { return val == el; });
								if (!shouldRun) {
									// restore element to it's init state
									var jel = $(el);
									jel.addClass('hide').html('').attr('data-running', 'false').data('running', false);
									if (_.isFunction(opts.onTaskCompleted)) {
										opts.onTaskCompleted(jel.data('task-id'), jel);
									}
								}
							});
						},
						error: function (xhr, ajaxOptions, thrownError) {
							window.clearInterval(interval);
						}
					});
				}
				window.setTimeout(poll, 50);
				interval = window.setInterval(poll, 2500);
			}
		}
	})()
};


