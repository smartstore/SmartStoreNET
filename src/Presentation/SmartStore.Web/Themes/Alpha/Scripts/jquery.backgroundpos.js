/* http://keith-wood.name/backgroundPos.html
   Background position animation for jQuery v1.1.0.
   Written by Keith Wood (kbwood{at}iinet.com.au) November 2010.
   Dual licensed under the GPL (http://dev.jquery.com/browser/trunk/jquery/GPL-LICENSE.txt) and 
   MIT (http://dev.jquery.com/browser/trunk/jquery/MIT-LICENSE.txt) licenses. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

var BG_POS = 'bgPos';

var usesTween = !!$.Tween;

if (usesTween) { // jQuery 1.8+
	$.Tween.propHooks['backgroundPosition'] = {
		get: function(tween) {
			return parseBackgroundPosition($(tween.elem).css(tween.prop));
		},
		set: function(tween) {
			setBackgroundPosition(tween);
		}
	};
}
else { // jQuery 1.7-
	// Enable animation for the background-position attribute
	$.fx.step['backgroundPosition'] = setBackgroundPosition;
};

/* Parse a background-position definition: horizontal [vertical]
   @param  value  (string) the definition
   @return  ([2][string, number, string]) the extracted values - relative marker, amount, units */
function parseBackgroundPosition(value) {
	var bgPos = (value || '').split(/ /);
	var presets = {center: '50%', left: '0%', right: '100%', top: '0%', bottom: '100%'};
	var decodePos = function(index) {
		var pos = (presets[bgPos[index]] || bgPos[index] || '50%').
			match(/^([+-]=)?([+-]?\d+(\.\d*)?)(.*)$/);
		bgPos[index] = [pos[1], parseFloat(pos[2]), pos[4] || 'px'];
	};
	if (bgPos.length == 1 && $.inArray(bgPos[0], ['top', 'bottom']) > -1) {
		bgPos[1] = bgPos[0];
		bgPos[0] = '50%';
	}
	decodePos(0);
	decodePos(1);
	return bgPos;
}

/* Set the value for a step in the animation.
   @param  fx  (object) the animation properties */
function setBackgroundPosition(fx) {
	if (!fx.set) {
		initBackgroundPosition(fx);
	}
	$(fx.elem).css('background-position',
		((fx.pos * (fx.end[0][1] - fx.start[0][1]) + fx.start[0][1]) + fx.end[0][2]) + ' ' +
		((fx.pos * (fx.end[1][1] - fx.start[1][1]) + fx.start[1][1]) + fx.end[1][2]));
}

/* Initialise the animation.
   @param  fx  (object) the animation properties */
function initBackgroundPosition(fx) {
	var elem = $(fx.elem);
	var bgPos = elem.data(BG_POS); // Original background position
	elem.css('backgroundPosition', bgPos); // Restore original position
	fx.start = parseBackgroundPosition(bgPos);
	fx.end = parseBackgroundPosition($.fn.jquery >= '1.6' ? fx.end :
		fx.options.curAnim['backgroundPosition'] || fx.options.curAnim['background-position']);
	for (var i = 0; i < fx.end.length; i++) {
		if (fx.end[i][0]) { // Relative position
			fx.end[i][1] = fx.start[i][1] + (fx.end[i][0] == '-=' ? -1 : +1) * fx.end[i][1];
		}
	}
	fx.set = true;
}

/* Wrap jQuery animate to preserve original backgroundPosition. */
$.fn.animate = function(origAnimate) {
	return function(prop, speed, easing, callback) {
		if (prop['backgroundPosition'] || prop['background-position']) {
			this.data(BG_POS, this.css('backgroundPosition') || 'left top');
		}
		return origAnimate.apply(this, [prop, speed, easing, callback]);
	};
}($.fn.animate);

})(jQuery);
