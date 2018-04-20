;

// Unobtrusive handler for inline discount rule partials
(function ($, window, document, undefined) {

	$('body').on('click', '.btn-save-discount-rule', function (e) {
		var btn = $(this),
			wrapper = btn.closest('.discount-rule-wrapper'),
			discountId = wrapper.data('discount-id'),
			requirementId = wrapper.data('requirement-id'),
			actionUrl = wrapper.data('action-url'),
			failMsg = wrapper.data('fail-msg'),
			successMsg = wrapper.data('success-msg');
		
		var data = { "discountId": discountId, "discountRequirementId": requirementId };
		wrapper.find("[data-routeparam]").each(function (i, el) {
			var $el = $(el);
			var routeValue;
			
			if ($el.data("routevalue")) {
				routeValue = window[$el.data("routevalue")]();
			}
			else {
				routeValue = $el.val();
			}
			data[$el.data("routeparam")] = routeValue;
		});

		$.ajax({
			cache: false,
			type: 'POST',
			url: actionUrl,
			data: data,
			success: function (data) {
				displayNotification(successMsg, "success");
				// notify parent if it's a new requirement
				if (requirementId == 0) {
					$("#discountRequirementContainer").trigger('smnewdiscountruleadded', [data.NewRequirementId]);
				}
			},
			error: function (xhr, ajaxOptions, thrownError) {
				window.displayNotification(failMsg, "error");
			}
		});
	});

})(jQuery, window, document);