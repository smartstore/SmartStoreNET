/*
*  Project: SmartStore column equalizer 
*  Author: Murat Cakir, SmartStore AG
*/
;
(function ($, window, document, undefined) {

    $.fn.extend({

        shrinkMenu: function (settings) {

            var options = {
                onChange: null,
                responsive: true
            }

            $.extend(options, settings);

            return this.each(function () {

            	var btn = $(this).attr("xxx-hidden", "hidden"); // the actual shrink container
            	var menu = btn.find('.dropdown-menu').first(); // the dropdown-menu within btn
                var btnWidth;
                var nav = btn.prev(); // the actual navbar should definitely be before the shrinker
                var containerWidth;

                function reset() {
                    nav.children(":hidden").removeAttr("hidden");
                    nav.find("> .dropdown > .dropdown-menu-right").removeClass("dropdown-menu-right");
                    btn.attr("hidden", "hidden");
                    if (menu) menu.empty();
                }

                function getContainerWidth() {
                    return parseFloat(nav.parent().css("width"));
                }

                function getButtonWidth() {
                    if (!btnWidth) {
                        btnWidth = btn.outerWidth(true);
                    }
                    return btnWidth;
                }

                function doShrink(resize, availWidth) {
                    containerWidth = getContainerWidth();
                    var totalButtonsWidth = nav.horizontalCushioning(true);

                    /* lis including dividers */
                    var navElements = $.makeArray(nav.children().not(btn)),
	                	exceedButtons = [],
	                	curWidth = 0,
	                	visibleButtons = _.filter(navElements, function (el) {
	                	    if (totalButtonsWidth <= containerWidth) {
	                	        el = $(el);
	                	        totalButtonsWidth += curWidth = el.outerWidth(true);
	                	        return (totalButtonsWidth > containerWidth) ? false : true;
	                	    }
	                	    else {
	                	        return false;
	                	    }
	                	});

                    var buttonShifter = (function () {
                        return {
                            shift: function (el) {
                                if (!el.hasClass("divider-vertical")) {
                                    var a = el.find(">a");
                                    var html = '<a class="dropdown-item" href="' + a.attr('href') + '">' + a.text() + '</li>';
                                    menu.append(html);
                                }

                                el.hide();
                            }
                        }
                    })();

                    if (visibleButtons.length < navElements.length) {
                        exceedButtons = _.difference(navElements, visibleButtons);
                        totalButtonsWidth -= curWidth;
                        curWidth = getButtonWidth();
                        while (totalButtonsWidth + curWidth > containerWidth) {
                            totalButtonsWidth -= $(_.last(visibleButtons)).outerWidth(true);
                            exceedButtons = [visibleButtons.pop()].concat(exceedButtons);
                        }
                        _.each(exceedButtons, function (val, i) {
                            buttonShifter.shift($(val));
                        });

                        // hide last visible item, if it's a divider
                        var lastVisible = $(visibleButtons[visibleButtons.length - 1]);
                        if (lastVisible.is(".divider-vertical")) {
                            lastVisible.hide();
                            lastVisible = lastVisible.prev();
                        }
                         
                        // as the last drop will likely exceed the layout boundaries
                        if (lastVisible.is(".dropdown")) {
                            lastVisible.find("> .dropdown-menu").addClass("dropdown-menu-right");
                        }
                    }

                    // finalize navbar
                    if (exceedButtons.length > 0) {
                        btn.removeAttr("hidden");
                        if (resize) {
                            if ($.isFunction(options.onChange)) options.onChange.call(this, menu);
                        }
                    }
                } 

            	// SHRINK IT!
                doShrink(false);
                
                if (options.responsive) {
                	EventBroker.subscribe("page.resized", function (msg, viewport) {
                        reset();
                        doShrink(true);
                    });
                }

            });

        }
    });

})(jQuery, this, document);