
(function ($) {
    
    $(function () {

        // init Globalize
        if (Globalize) {
            // Ask ASP.NET what culture we prefer, because we stuck it in a meta tag
            var data = $("meta[name='accept-language']").attr("content")
            // Tell jQuery to figure it out also on the client side.
            Globalize.culture(data || "en-US");
            if ($.fn.datetimepicker) {
            	// globalize bootstrap datepicker
            	var c = Globalize.culture().calendars.standard;
            	$.fn.datetimepicker.defaults = {
            		format: c.patterns['d'],
            		weekStart: c.firstDay,
            		autoclose: true,
            		todayHighlight: true
            	};
            	$.fn.datetimepicker.dates['glob'] = {
            		days: c.days.names,
            		daysShort: c.days.namesShort,
            		daysMin: c.days.namesAbbr,
            		months: c.months.names,
            		monthsShort: c.months.namesAbbr,
            		today: "Today" // TODO: Localize
            	};
            }

            // Use the Globalization plugin to parse some values
            if ($.validator) {
                jQuery.extend($.validator.methods, {
                    number: function (value, element) {
                        return this.optional(element) || !isNaN(Globalize.parseFloat(value));
                    },
                    date: function (value, element) {
                        return this.optional(element) || !isNaN(Globalize.parseDate(value));
                    },
                    range: function (value, element, param) {
                        var val = Globalize.parseFloat(value);
                        return this.optional(element) || (val >= param[0] && val <= param[1]);
                    }
                });
            }
        }

    });


})(jQuery);