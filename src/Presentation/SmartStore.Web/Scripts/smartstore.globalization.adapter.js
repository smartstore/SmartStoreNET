
(function ($) {
	$(function () {
		var g = SmartStore.globalization;

		if (typeof g === undefined)
			return;

		// Adapt to moment.js
		if (typeof moment !== undefined) {
			var dtf = g.culture.dateTimeFormat;
			moment.defineLocale('glob', {
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
				},
				// TODO: (mc) localize!
				calendar: {
					sameDay: '[Today at] LT',
					nextDay: '[Tomorrow at] LT',
					nextWeek: 'dddd [at] LT',
					lastDay: '[Yesterday at] LT',
					lastWeek: '[Last] dddd [at] LT',
					sameElse: 'L'
				},
				// TODO: (mc) localize!
				relativeTime: {
					future: 'in %s',
					past: '%s ago',
					s: 'a few seconds',
					m: 'a minute',
					mm: '%d minutes',
					h: 'an hour',
					hh: '%d hours',
					d: 'a day',
					dd: '%d days',
					M: 'a month',
					MM: '%d months',
					y: 'a year',
					yy: '%d years'
				},
				// TODO: (mc) localize!
				dayOfMonthOrdinalParse: /\d{1,2}(st|nd|rd|th)/,
				ordinal: function (number) {
					var b = number % 10,
						output = (~~(number % 100 / 10) === 1) ? 'th' :
						(b === 1) ? 'st' :
						(b === 2) ? 'nd' :
						(b === 3) ? 'rd' : 'th';
					return number + output;
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