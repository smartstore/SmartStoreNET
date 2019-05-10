/* smartstore.system.js
-------------------------------------------------------------- */
;
(function ($) {

    function detectTouchscreen() {
        var result = false;
        if (window.PointerEvent && ('maxTouchPoints' in navigator)) {
            // if Pointer Events are supported, just check maxTouchPoints
            if (navigator.maxTouchPoints > 0) {
                result = true;
            }
        } else {
            // no Pointer Events...
            if (window.matchMedia && window.matchMedia("(any-pointer:coarse)").matches) {
                // check for any-pointer:coarse which mostly means touchscreen
                result = true;
            } else if (window.TouchEvent || ('ontouchstart' in window)) {
                // last resort - check for exposed touch events API / event handler
                result = true;
            }
        }
        return result;
    }

    Modernizr.touchevents = detectTouchscreen();

    if (Modernizr.touchevents) {
        window.document.documentElement.classList.remove("no-touchevents");
        window.document.documentElement.classList.add("touchevents");
    }

	var formatRe = /\{(\d+)\}/g;
	
	String.prototype.format = function() {
	    var s = this, args = arguments;
	    return s.replace(formatRe, function(m, i) {
	        return args[i];
	    });
	};

	// define noop funcs for window.console in order
	// to prevent scripting errors
	var c = window.console = window.console || {};
	function noop() { };
	var funcs = ['log', 'debug', 'info', 'warn', 'error', 'assert', 'dir', 'dirxml', 'group', 'groupEnd', 
					'time', 'timeEnd', 'count', 'trace', 'profile', 'profileEnd'],
		flen = funcs.length,
		noop = function(){};
	while (flen) {
		if (!c[funcs[--flen]]) {
			c[funcs[flen]] = noop;	
		}
	}
		
	// define default secure-casts
	jQuery.extend(window, {
			
		toBool: function(val) {
			var defVal = typeof arguments[1] === "boolean" ? arguments[1] : false;
			var t = typeof val;
			if (t === "boolean") {
				return val;	
			}
			else if (t === "string") {
				switch (val.toLowerCase()) {
					case "1": case "true": case "yes": case "on": case "checked":
						return true;
					case "0": case "false": case "no": case "off":
						return false;
					default:
						return defVal;
				}
			}
			else if (t === "number" ) {
				return Boolean(val);	
			}
			else if (t === "null" || t === "undefined" ) {
				return defVal;	
			}
			return defVal;
		},
				
		toStr: function(val) {
			var defVal = typeof arguments[1] === "string" ? arguments[1] : "";
			if (!val || val === "[NULL]") {
				return defVal;	
			}
			return String(val) || defVal;
		},

		toInt: function(val) {
			var defVal = typeof arguments[1] === "number" ? arguments[1] : 0;
			var x = parseInt(val);
			if (isNaN(x)) {
				return defVal;	
			}
			return x;
		},
			
		toFloat: function(val) {
			var defVal = typeof arguments[1] === "number" ? arguments[1] : 0;
			var x = parseFloat(val);
			if (isNaN(x)) {
				return defVal;	
			}
			return x;
		}				
	});
	
	// provide main app namespace
	window.SmartStore = {};
})( jQuery );
