/**
 * bootstrap-datepicker 'Globalize' adapter
 * Sam Zurcher <sam@orelias.ch>
 */
; (function ($) {
    var c = Globalize.culture().calendars.standard;
	$.fn.datepicker.dates['glob'] = {
		days: c.days.names,
		daysShort: c.days.namesShort,
		daysMin: ["So", "Mo", "Di", "Mi", "Do", "Fr", "Sa", "So"],
		months: ["Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember"],
		monthsShort: ["Jan", "Feb", "Mär", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dez"],
		today: "Heute"
	};
}(jQuery));
