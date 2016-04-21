/*
 * Depends:
 *   jquery.ui.position.js
 */
(function( $, undefined ) {

var iconClasses = {
	left: "fa fa-chevron-left", 
	right: "fa fa-chevron-right",
	up: "fa fa-chevron-up",
	down: "fa fa-chevron-down"
};

$.ScrollButton = function(el, buttons, target, options) {
	var self = this;
	
	el.data("ScrollButton", this);
	
	var opts = $.extend({}, options);
	
	// make "corrections" to important options
	opts.nearSize = Math.max(12, opts.nearSize || (opts.smallIcons ? 16 : 32));
	opts.farSize = Math.max(12, opts.farSize || (opts.smallIcons ? 16 : 32));
	opts.offset = _.isNumber(opts.offset) ? opts.offset : 0;
	this.direction = opts.direction = el.hasClass("sb-dir-left") ? 'left' : 'right';
	this.target = target;

	var initialized = false;
	var enabled = !el.hasClass("disabled");
	var inButton = false;
	var inTarget = false;
	var outside = opts.position === "outside";
	var offOpacity = opts.showButtonAlways ? 40 : 0;
	var onOpacity = !target ? 100 : (opts.showButtonAlways ? 80 : 40);
	var hoverOpacity = 100;
	var removableOpacityClasses = "o{0} o{1} o{2}".format(hoverOpacity, onOpacity, offOpacity);
	
	var isVert = opts.direction == "up" || opts.direction == "down";
	
	this._init = function() {
	    el.addClass("scroll-button btn " + (opts.smallIcons ? "small" : "large"));
		
		// set size
		if (isVert) {
			el.css({ width: opts.nearSize, height: opts.farSize });
		}
		else {
			el.css({ width: opts.farSize, height: opts.nearSize });
		}
		
		// apply 'position'
		if (target) {
			var offset = [opts.offset, 0];
			var pos = { of: target, collision: "none" };
			var cornerSide, nullBorderSide;
			var dir = opts.direction;
			var atmy = "";
			
			buttons[dir] = el;
			
			// negate offset for 'near' sides if outside
			if (dir == "left" || dir == "up") {
				if (opts.position == "outside") offset[0] *= -1;	
			}
			
			// negate offset for 'far' sides if inside
			if (dir == "down" || dir == "right") {
				if (opts.position == "inside") offset[0] *= -1;	
			}
			
			// reverse offset for vertical arrows
			if (isVert) {
				offset.reverse();	
			}
			
			switch (opts.direction) {
				case "up": 
					cornerSide = (outside ? "top" : "bottom");
					nullBorderSide = (outside ? "bottom" : "top");
					pos.at = "center top";
					pos.my = (outside ? "center bottom" : pos.at);
					break;
				case "down": 
					cornerSide = (outside ? "bottom" : "top");
					nullBorderSide = (outside ? "top" : "bottom");
					pos.at = "center bottom";
					pos.my = (outside ? "center top" : pos.at);
					break;  
				case "right": 
					cornerSide = (outside ? "right" : "left");
					nullBorderSide = (outside ? "left" : "right");
					pos.at = "right center";
					pos.my = (outside ? "left center" : pos.at);
					break;  
				default: 
					cornerSide = (outside ? "left" : "right");
					nullBorderSide = (outside ? "right" : "left");
					pos.at = "left center";
					pos.my = (outside ? "right center" : pos.at);
			}
			
			if (opts.autoPosition) {
				pos.offset = offset.join(" ");
				el.position(pos);
				
				if (opts.responsive) {
				    EventBroker.subscribe("page.resized", function (data) {
				        el.position(pos);
				    });
				}
			}
			
		} // if (target)
		
		el.text("").append('<i class="' + iconClasses[opts.direction] + '"></i>');
		
		el.addClass("sb-dir-" + opts.direction);

		el.bind({
			"mouseenter.scrollbutton": function() { 
				inButton = true;
				// ohne defer würde target.enter HIERNACH gefeuert, dat wollen wir nicht.
				_.defer(function() { self.setState("hovered") }, 1);
				if (enabled && $.isFunction(opts.enter)) {
					opts.enter.call(this, opts.direction);
				} 
			}, 
			"mouseleave.scrollbutton": function() {
				self.setState("on");
				inButton = false;
				if ($.isFunction(opts.leave)) {
					opts.leave.call(this, opts.direction);
				} 
			},
			"mousedown.scrollbutton": function() { 
				self.setState("active"); 
			},
			"mouseup.scrollbutton": function() { 
				self.setState("hovered");
			},
			"click.scrollbutton": function(evt) { 
				evt.preventDefault();
				if (enabled && $.isFunction(opts.click)) {
					opts.click.call(this, opts.direction);
				} 
			}
		});
		
		self.enable(enabled);
		
		el.removeClass("transparent invisible hide");
		
		initialized = true;
	}; // init
	
	this.setInTarget = function(value) {
		inTarget = value;	
	}
	
	// to call this method, fetch the plugin instance from the
	// jq-Element first: $("#myscrollbutton").data("ScrollButton").enable(false)
	this.enable = function(enable) {
		enabled = enable;
		if (enabled) {
			self.setState(inButton ? "hovered" : (inTarget || !target ? "on" : "off"));
			el.removeClass("disabled");
		}
		else {
			el.addClass("disabled");
			self.setState("off", true);
		}
	};
	
	this.setState = function(state, selfCall /*internal*/) {
		if (!selfCall && !enabled) return;
		if (state === "off") {
			self.setOpacity(offOpacity);	
		}
		else if (state === "on") {
			self.setOpacity(onOpacity);	
		}
		else if (state === "hovered") {
			self.setOpacity(hoverOpacity);		
		}
		else if (state === "active") {
			self.setOpacity(hoverOpacity);	
		}
	};
	
	this.setOpacity = function(o) {

		if (initialized) {
			el.stop(true, true).animate( {opacity: o/100}, 150 );
		}
		else {
			el.css("opacity", o/100);
		}

		return self;	
	}
	
	this._init();	
}

$.ScrollButton.defaults = {
	// int (px) or null.
	nearSize: null,
	// int (px) or null.
	farSize: null,
	// when true, renders the smaller jQuery ui-icons
	smallIcons: false,
	// a selector (string | element | jqObj) if the buttons should be coupled
	// to a target panel or 'null', if buttons should be standalone.
	target: null,
	// when true, uses jQuery position plugin to auto-align the button with the target (if available) 
	autoPosition: true,
    responsive: true,
	// inside | outside. When autoPosition is true, aligns the button within or outside
	// the boundaries of the target.
	position: "inside",
	// [val]px. When autoPosition is true, shifts the button by this amount 
	// either negative or positive (depends on 'position')
	offset: 0,
	// When true, a 'very transparent' button is visible even the target is not entered.
	// If no target is set, 'false' has no effect, as there would be no chance
	// to 'get the button back'.
	showButtonAlways: false,
	// String (left | right | up | down) or null. Defines the direction of the button.
	// This affects icon-rendering, auto-positioning, corners, borders etc.
	direction: null,
	// When false, the button is dimmed/hidden and cannot be clicked.
	enabled: true,
	// function( string direction ) returns boolean. Is called for every button within
	// a target when it is entered. Return true, if the button should be enabled or false for disabling.
	canScroll: null,
	// function( string direction ). Slide/scroll here.
	click: null,
	enter: null,
	leave: null
}

$.fn.extend( {
	
	scrollButton: function(options) {
		options = $.extend( {}, $.ScrollButton.defaults, options );
		var buttons = { /* left: null, right: null, up: null, down: null */ };
		var target = options.target ? $(options.target) : null;
		
		if (target) {
			var scrollFn = $.isFunction(options.canScroll) ? options.canScroll : function() { return true; };
			target.bind({
				"mouseenter.scrollbutton": function() {
					_.each(buttons, function(val, i){
						var plugin = val.data("ScrollButton");
						plugin.setInTarget(true);
						plugin.setState("on");
					});
				},
				"mouseleave.scrollbutton": function() {
					_.each(buttons, function(val, i){
						var plugin = val.data("ScrollButton");
						plugin.setInTarget(false);
						plugin.setState("off", true);
					});
				}
			});
		}; // if (target)
		
		return this.each(function() {
			(new $.ScrollButton($(this), buttons, target, options));
		});
	}
	
});

})( jQuery );