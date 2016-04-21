/*
*  Project: SmartStore menu 
*  Author: Murat Cakir, SmartStore AG
*/

; (function ($, window, document, undefined) {

    var pluginName = 'navbar';

    function Navbar(element, options) {
        var self = this;

        // to access the DOM elem from outside of this constructor
        this.element = element;
        var el = this.el = $(element);

        // support metadata plugin
        var meta = $.metadata ? $.metadata.get(element) : {};
        // 'this.options' ensures we can reference the merged instance options from outside
        var opts = this.options = $.extend({}, options, meta || {});

        this.init = function () {
			
			var drop = el.find("> .dropdown-menu"),
				inner = drop.find('.dropdown-menu-inner'), 
				ul = inner.find('.drop-list'), 
				li = null, 
				a = el.find("> a").removeAttr("data-toggle"), /* remove default bootstrap toggle attribute */
				ulSub = null;
			
        	// init mouse aiming towards submenu
			drop.menuAim(opts.menuAim);

            inner.data("initial-height", inner.height());
            inner.css("height", inner.data("initial-height"));

            var adjustHeight = function (el, height, animate) {
                if (Modernizr.csstransitions || !animate) {
                    el.height(height);
                }
                else {
                    if (height != parseFloat(el.css("height"))) {
                    	el.stop(true, true).animate({ height: height }, 300, "ease-out");
                	}
                }
            };

			el.on("mouseenter", function() { $(this).addClass("open"); } )
			  .on("mouseleave", function() { $(this).removeClass("open"); } );
			
            // event
            var timeout = null;
            drop.on("mouseenter", "li.dropdown-submenu", function (e) {
                var li = $(this);

                ulSub = li.find("> .dropdown-menu");
                if (!ulSub.length) {
                    adjustHeight(inner, inner.data("initial-height"));
                }
                else {
                    window.clearTimeout(timeout);
                   	drop.addClass("expanded");
                   	
                    if (ulSub.data("initial-height") == null) {
                        ulSub.data("initial-height", ulSub.height());
                    }

                    if (ulSub.outerHeight(true) >= inner.outerHeight(true)) {
                        // expand
                        adjustHeight(inner, ulSub.outerHeight(true), true);
                    }
                    else {
                        var vCush = ulSub.verticalCushioning();
                        var innerHeight = Math.max(inner.data("initial-height"), ulSub.data("initial-height") + vCush);
                        var ulSubHeight = Math.max(inner.data("initial-height") - vCush, ulSub.data("initial-height"));

                        adjustHeight(inner, innerHeight, true);
                        adjustHeight(ulSub, ulSubHeight, false);
                    }
        		}
            });

            drop.on("mouseleave", ".drop-list > li", function (e) {
                function close() {
	                drop.removeClass("expanded");
	                adjustHeight(inner, inner.data("initial-height"), true);
                };
                window.clearTimeout(timeout);
                timeout = window.setTimeout(close, 600);
            });

        };

        this.initialized = false;
        this.init();
        this.initialized = true;
    }

    Navbar.prototype = {
        // [...]
    }

    // the global, default plugin options
    var defaults = {
    	menuAim: {
    		rowSelector: "li.drop-list-item",
    		submenuSelector: ".dropdown-submenu",
    		activate: function (item) {
    			$(item).addClass("aimed");
    		},
    		deactivate: function (item) {
    			$(item).removeClass("aimed");
    		}
    	}
    }
    $[pluginName] = { defaults: defaults };


    $.fn[pluginName] = function (options) {

        return this.each(function () {
            if (!$.data(this, pluginName)) {
                options = $.extend( true, {}, $[pluginName].defaults, options );
                $.data(this, pluginName, new Navbar(this, options));
            }
        });

    }
    
	/* APPLY TO STANDARD MENU ELEMENTS
	* =================================== */

	$(function () {
		//$('.navbar ul.nav-smart > li.dropdown').menu();
	})

})(jQuery, window, document);
