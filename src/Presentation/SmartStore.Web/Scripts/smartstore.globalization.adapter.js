
(function ($) {
	$(function () {
		var g = SmartStore.globalization;

		if (typeof g === undefined)
			return;

		// Adapt to moment.js
		if (typeof moment !== undefined) {
			var dtf = g.culture.dateTimeFormat;
			moment.defineLocale('glob', {
				parentLocale: moment.locale(),
				months: dtf.months.names,
				monthsShort: dtf.months.namesAbbr,
				weekdays: dtf.days.names,
				weekdaysShort: dtf.days.namesShort,
				weekdaysMin: dtf.days.namesAbbr,
				longDateFormat: {
					LT: g.convertDatePatternToMomentFormat(dtf.patterns['t']),
					LTS: g.convertDatePatternToMomentFormat(dtf.patterns['T']),
					L: g.convertDatePatternToMomentFormat(dtf.patterns['d']),
					LL: g.convertDatePatternToMomentFormat(dtf.patterns['D']),
					LLL: g.convertDatePatternToMomentFormat(dtf.patterns['f']),
					LLLL: g.convertDatePatternToMomentFormat(dtf.patterns['F'])
				},
				week: {
					dow: dtf.firstDay, // Monday is the first day of the week.
					doy: 4  // The week that contains Jan 4th is the first week of the year.
				}
			});
		}

		// Adapt to jQuery validate
		if (typeof $.validator !== undefined) {
			$.extend($.validator.methods, {
				number: function (value, element) {
					return this.optional(element) || !isNaN(g.parseFloat(value));
				},
				date: function (value, element) {
					if (this.optional(element)) return true;
					var validPatterns = ['L LTS', 'L LT', 'L', 'LTS', 'LT'];
					return moment(value, $(element).data('format') || validPatterns, true /* exact */).isValid();
				},
				range: function (value, element, param) {
					var val = g.parseFloat(value);
					return this.optional(element) || (val >= param[0] && val <= param[1]);
				}
			});
		}
	});
})( jQuery );