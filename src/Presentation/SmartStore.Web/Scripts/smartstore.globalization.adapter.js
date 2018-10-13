
(function ($) {
	$(function () {
		var g = SmartStore.globalization;

		if (typeof g === undefined)
			return;

		// Adapt to moment.js
		if (typeof moment !== undefined) {
			var dtf = g.culture.dateTimeFormat;

			var glob = {
				parentLocale: moment.locale(),
				months: dtf.months.names,
				monthsShort: dtf.months.namesAbbr,
				weekdays: dtf.days.names,
				weekdaysShort: dtf.days.namesAbbr,
				weekdaysMin: dtf.days.namesShort,
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
				// Unfortunately .NET cannot handle native digits in dates (like moment.js does).
				// So we need to return the original system digits here
				// (instead of - e.g. - eastern arabic digits).
				preparse: function (string) {
					return string;
				},
				postformat: function (string) {
					return string;
				}
			};

			var meridiem = dtf.AM && dtf.AM.length && dtf.PM && dtf.PM.length;
			if (meridiem) {
				// AM/PM: we cannot rely on moment.js definitions, because there are some discrepancies
				// to .NET Framework's DateTime handling. Therefore we 'teach' moment how it's done in .NET.
				glob.meridiemParse = new RegExp(dtf.AM[0] + '|' + dtf.PM[0], 'i');
				glob.meridiem = function (hour, minute, isLower) {
					var d = hour < 12 ? dtf.AM : dtf.PM;
					return isLower ? d[1] : d[0];
				}
			}

			moment.defineLocale('glob', glob);
		}

		// Adapt to jQuery validate
        if (typeof $.validator !== undefined) {

            function _getValue(value, element) {
                return $(element).is('[type=range]')
                    ? parseFloat(value)
                    : g.parseFloat(value);
            }

            $.extend($.validator.methods, {
                number: function (value, element) {
                    return this.optional(element) || !isNaN(_getValue(value, element));
				},
				date: function (value, element) {
					if (this.optional(element)) return true;
					var validPatterns = ['L LTS', 'L LT', 'L', 'LTS', 'LT'];
					return moment(value, $(element).data('format') || validPatterns, true /* exact */).isValid();
				},
                range: function (value, element, param) {
                    var val = _getValue(value, element);
					return this.optional(element) || (val >= param[0] && val <= param[1]);
				}
			});
		}
	});
})( jQuery );