
(function (productFilter, $, undefined) {

    productFilter.init = function (containerSelector, dialogSelector) {
    	var container = $(containerSelector),
    		dialog = $(dialogSelector);

        InitEvents(container, dialog);

        // get filter mask
        container.find('.filter-product-form').doAjax({
            type: 'GET',
            smallIcon: container.find('.listbox'),
            callbackSuccess: function (resp) {
                container.find('.listbox').html(resp);
            }
        });
    }

    
    function InitEvents(container, dialog) {
        // show/hide filter links
    	container.on('click', ".filter-group:not(.static) .name", function () {
    		var group = $(this).parent(),
				items = group.find('.data');

    		if (group.hasClass('expanded')) {
    			items.slideUp('fast', function () {
    				group.removeClass('expanded');
    			});
    		}
    		else {
    			items.slideDown('fast', function () {
    				group.addClass('expanded');
    			});
    		}
    	});

    	// show multi select dialog
    	container.on('click', '.more', function () {
    		var form = container.find('.filter-product-form');

        	form.find('[name=filterMultiSelect]').val($(this).attr('data-multiselect'));
        	dialog.find('.modal-body').empty();

        	form.doAjax({
        		type: 'GET',
        		url: form.attr('data-multiselecturl'),
        		smallIcon: dialog.find('.modal-body'),
        		callbackSuccess: function (resp) {
        			dialog.find('.modal-body').html(resp);
        			form.find('[name=filterMultiSelect]').val('');
        		}
        	});

        	var caption = $(this).closest('.filter-group').find('.name span').text();
        	dialog.find('.modal-header .caption').html(caption);
    	});

    	// remove all checked filter values
    	dialog.on('click', '.remove-checkmarks', function () {
            $(this).blur();
            dialog.find('.modal-body input[type=checkbox]').removeAttr('checked');
    	});

    	// submit multi select filtering
    	dialog.on('click', '.btn-primary', function () {
    		var form = dialog.find('.multi-select-form'),
				nameEl = form.find('[name=filter]'),
    			filter = nameEl.val() ? $.parseJSON(nameEl.val()) : null,
				values = [];

    		if (filter != null) {
    			for (var i = 0; i < filter.length; ++i)
    				values.push(JSON.stringify(filter[i]));
    		}

    		dialog.find(':checkbox:checked').each(function () {
    			values.push(this.value);
    		});

    		var valueString = '[' + values.join(',') + ']';

    		nameEl.val(valueString);
    		form.submit();
    	});
    }

}(window.productFilter = window.productFilter || {}, jQuery));