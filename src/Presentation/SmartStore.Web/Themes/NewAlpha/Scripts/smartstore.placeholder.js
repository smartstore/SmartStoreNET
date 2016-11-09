/*
* Placeholder plugin for jQuery
* ---
* Copyright 2010, Daniel Stocks (http://webcloud.se)
* Released under the MIT, BSD, and GPL Licenses.
*/
(function($) {
	
    function Placeholder(input) {
        this.input = input;
        if (input.attr('type') == 'password') {
            this.handlePassword();
        }
        // Prevent placeholder values from submitting
        $(input[0].form).submit(function() {
            if (input.hasClass('placeholder') && input[0].value == input.attr('placeholder')) {
                //input[0].value = '';
                $(input[0]).plVal("");
            }
        });
    };
    
    Placeholder.prototype = {
        show : function(loading) {
            // FF and IE saves values when you refresh the page. If the user refreshes the page with
            // the placeholders showing they will be the default values and the input fields won't be empty.
            if (this.input[0].value === '' || (loading && this.valueIsPlaceholder())) {
                if (this.isPassword) {
                    try {
                        this.input[0].setAttribute('type', 'text');
                    } catch (e) {
                        this.input.before(this.fakePassword.show()).hide();
                    }
                }
                this.input.addClass('placeholder');
                //this.input[0].value = this.input.attr('placeholder');
                $(this.input[0]).plVal(this.input.attr('placeholder'));
            }
        },
        hide : function() {
            if (this.valueIsPlaceholder() && this.input.hasClass('placeholder')) {
                this.input.removeClass('placeholder');
                //this.input[0].value = '';
                $(this.input[0]).plVal('');
                if (this.isPassword) {
                    try {
                        this.input[0].setAttribute('type', 'password');
                    } catch (e) { }
                    // Restore focus for Opera and IE
                    this.input.show();
                    this.input[0].focus();
                }
            }
        },
        valueIsPlaceholder : function() {
            return this.input[0].value == this.input.attr('placeholder');
        },
        handlePassword: function() {
            var input = this.input;
            input.attr('realType', 'password');
            this.isPassword = true;
        }
    };
    
	// Replace the val function to never return placeholders
	$.fn.plVal = $.fn.val;
	$.fn.val = function(value) {
		if(this[0]) {
			var el = $(this[0]);
			if(value != undefined) {
				var currentValue = el.plVal();
				var returnValue = $(this).plVal(value);
				if (el.hasClass('placeholder') && currentValue == el.attr('placeholder')) {
					el.removeClass('placeholder');
				}
				return returnValue;
			}

			if (el.hasClass('placeholder') && el.plVal() == el.attr('placeholder')) {
				return '';
			} else {
				return el.plVal();
			}
		}
		return undefined;
	};
    
    var NATIVE_SUPPORT = Modernizr.input.placeholder;
    
    $.fn.placeholder = function() {
        return NATIVE_SUPPORT ? this : this.each(function() {
            var input = $(this);
            var placeholder = new Placeholder(input);
            placeholder.show(true);
            input.bind("blur dragleave", function() {
            	placeholder.show(false);	
            });
            input.bind("dragend", function() {
            	window.setTimeout(function() { placeholder.show(false); }, 1);	
            });
            input.bind("focus dragenter", function() {
            	placeholder.hide();	
            });

            // On page refresh, IE doesn't re-populate user input
            // until the window.onload event is fired.
            if (navigator.isIE) {
                $(window).load(function() {
                    if(input.val()) {
                        input.removeClass("placeholder");
                    }
                    placeholder.show(true);
                });
                // What's even worse, the text cursor disappears
                // when tabbing between text inputs, here's a fix
                input.focus(function() {
                    if(this.value == "") {
                        var range = this.createTextRange();
                        range.collapse(true);
                        range.moveStart('character', 0);
                        range.select();
                    }
                });
            }
        });
    };
    
    // call it globally on all inputs
    // with a placeholder attribute
    $(function () {
        $("input[placeholder]").placeholder();
    });

})(jQuery);