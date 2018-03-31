var currentModalId = "";
function closeModalWindow() {
    var modal = $('#' + currentModalId).data('modal');
    if (modal)
        modal.hide();
    return false;
}
function openModalWindow(modalId) {
    currentModalId = modalId;
    $('#' + modalId).modal('show');
}

// global Admin namespace
SmartStore.Admin = {
	modelTrees: {},
	checkboxCheck: function (obj, checked) {
		if (checked)
			$(obj).attr('checked', 'checked');
		else
			$(obj).removeAttr('checked');
	},
	checkAllOverriddenStoreValue: function (obj) {
		$('.multi-store-override-option').each(function (i, el) {
			SmartStore.Admin.checkboxCheck(el, obj.checked);
			SmartStore.Admin.checkOverriddenStoreValue(el);
		});
	},
	checkOverriddenStoreValue: function (el) {
		var checkbox = $(el);
		var parentSelector = checkbox.data('parent-selector'),
			parent = parentSelector ? $(parentSelector) : checkbox.closest('.multi-store-setting-group').find('> .multi-store-setting-control'),
			checked = checkbox.is(':checked');

		parent.find('input:not([type=hidden]), select').each(function (i, el) {
			var input = $(el);
			var tbox = input.data('tTextBox');

			if (tbox) {
				checked ? tbox.enable() : tbox.disable();
			}
			else {
				checked ? input.removeAttr('disabled') : input.attr('disabled', true);
			}
		});
	},
	movePluginActionButtons: function() {
		// Move plugin specific action buttons (like 'Save') to top header section
		var pluginActions = $('.plugin-config-container .plugin-actions');
		if (pluginActions) {
			// Action buttons do exist: prepend them to header
			pluginActions.detach().prependTo('.section-header .options').on('click', 'button[type=submit]', function (e) {
				// On SubmitButtonClick, post the form programmatically, as the button is not a child of the form anymore...
				var form = $('.plugin-config-container form').first();
				if (form) {
					// ...but first add a hidden input to the form with button's name and value to mimic button click WITHIN the form.
					var btn = $(e.currentTarget);
					form.prepend($('<input type="hidden" name="' + btn.attr('name') + '" value="' + btn.attr('value') + '" />'));
					form.submit();
				}
			});
		}
	},
	togglePanel: function(el /* the toggler */, animate) {
		var ctl = $(el),
			show = ctl.is(':checked'),
			reverse = ctl.data('toggler-reverse');

		if (reverse) show = !show;

		var duration = animate ? 200 : 0;

		function afterShow() { $(this).addClass('expanded'); }
		function afterHide() { $(this).removeClass('expanded'); }

		$(ctl.data('toggler-for')).each(function (i, cel) {
			var pnl = $(cel),
				isGroup = pnl.is('tbody, .collapsible-group');

			pnl.addClass('collapsible');
			if (isGroup) pnl.addClass('collapsible-group')

			if (show) {
				if (!isGroup) {
					pnl.show(duration, afterShow);
				}
				else {
					var targets = pnl.children()
						.hide() // initially hide all children asap
						.filter(':not(.collapsible), .collapsible.expanded'); // fetch only expandable items
					pnl.show(0, afterShow); // first, show panel group asap (otherwise we won't see any animation)
					targets.show(duration); // animate all items
				}
			}
			else {
				if (!isGroup) {
					pnl.hide(duration, afterHide);
				}
				else {
					// hide all children (animated)
					pnl.children().hide(duration).promise().done(function () {
						pnl.hide(0, afterHide); // last, hide panel group asap
					});
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
						//dataType: 'json',
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
										row1.append($('<div class="text float-left">' + (task.message || opts.defaultProgressMessage) + '</div>'));
										row1.append($('<div class="percent float-right">' + (task.percent >> 0 ? task.percent + ' %' : "") + '</div>'));
										var row2 = $('<div class="loading-bar mt-2"></div>').appendTo(el);
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
								var shouldRun = _.find(runningElements, function (val) { return val === el; });			
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
							console.error(thrownError);
						}
					});
				}
				window.setTimeout(poll, 50);
				interval = window.setInterval(poll, 2500);
			}
		}
	})()
};