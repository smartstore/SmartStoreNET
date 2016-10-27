(function ($) {
    // Depends on:
    // bootstrap 4
    // jquery.scrollTo.js

    $.fn.extend({
        megaMenu: function (settings) {

            var defaults = {
                productRotatorInterval: 4000,
                productRotatorDuration: 300,
                productRotatorCycle: false,
                productRotatorAjaxUrl: ""
            };

            var settings = $.extend(defaults, settings);

            return this.each(function () {

                var megamenuContainer = $(this);
                var megamenu = $(".megamenu", megamenuContainer);
                var megamenuNext = $(".megamenu-nav--next", megamenuContainer);
                var megamenuPrev = $(".megamenu-nav--prev", megamenuContainer);
                var megamenuDropdownContainer = $('.megamenu-dropdown-container');
                var navElems = megamenu.find(".nav .nav-item");	// li
                var zoomContainer = $(".zoomContainer");		// needed to fix elevateZoom z-index problem e.g. in product detail gallery
                var closingTimeout = 0;							// timeout to handle delay of dropdown closing
                var openTimeout;								// timeout to handle opening attempts in tryOpen
                var tempLink; 		// for temporary storage of the link that was passed into tryOpen, its needed to determine whether a new link was passed into tryOpen so the timeout for the old link can be cleared

                function closeMenuHandler(link, closeImmediatly) {
                    closingTimeout = setTimeout(function () { closeNow(link) }, 250);
                }

                function closeNow(link) {
                    $(link.data("target")).removeClass("open");

                    if (link.hasClass("dropdown-toggle")) {
                        link.closest("li").removeClass("active");
                    }

                    zoomContainer.css("z-index", "999");
                }

                function tryOpen(link) {

                    // if new link was passed into function clear tryOpen-timeout
                    if (tempLink && link.data("target") != tempLink.data("target")) {
                        clearTimeout(openTimeout);
                    }

                    // just open if there are no open menus, else wait and try again as long as there is a menu open
                    if (navElems.hasClass('active') || $(".dropdown-container").hasClass('open')) {
                        tempLink = link;

                        openTimeout = setTimeout(function () { tryOpen(link); }, 50);
                    } else {
                        clearTimeout(openTimeout);

                        $(link.data("target")).addClass("open");

                        if (link.hasClass("dropdown-toggle")) {
                            link.closest("li").addClass("active");
                        }

                        initRotator(link.data("target"));

                        zoomContainer.css("z-index", "0");
                    }
                }

                $(".mega-menu-dropdown").on('mouseenter', function () {
                    clearTimeout(closingTimeout);
                })
                .on('mouseleave', function () {
                    var targetId = $(this).parent().attr("id");
                    var link = megamenu.find("[data-target='#" + targetId + "']");

                    closeMenuHandler(link);
                });

                navElems.on("mouseenter", function () {
                    var link = $(this).find(".nav-link");

                    // if correct dropdown is already open then don't open it again
                    var opendMenuSelector = $(".nav-item.active .nav-link").data("target");

                    if (opendMenuSelector == link.data("target")) {
                        clearTimeout(closingTimeout);
                        return;
                    }

                    tryOpen(link);
                })
			  	.on("mouseleave", function () {
			  	    var link = $(this).find(".nav-link");

			  	    closeMenuHandler(link);
			  	});

                //oh, oh, oh, oh, can't touch this ;-/
                var hammertime = new Hammer($(".megamenu")[0]);
                hammertime.add(new Hammer.Pan({ direction: Hammer.DIRECTION_HORIZONTAL, threshold: 80, pointers: 1 }));

                hammertime.on('panend', function (ev) {
                    if (ev.direction == Hammer.DIRECTION_LEFT) { megamenuNext.trigger('click'); }
                    if (ev.direction == Hammer.DIRECTION_RIGHT) { megamenuPrev.trigger('click'); }
                });

                megamenuContainer.evenIfHidden(function (el) {

                    var scrollCorrection = null;
                    var lastVisibleElem = null;
                    var firstVisibleElem = null;
                    var isFirstItemVisible = true;
                    var isLastItemVisible = false;

                    megamenuContainer.find('ul').wrap('<div class="nav-slider" style="overflow: hidden;position: relative;" />');

                    getCurrentNavigationElements();

                    megamenuNext.click(function (e) {

                        e.preventDefault();

                        var scrollDelta = $(lastVisibleElem).position().left + 55 + $(lastVisibleElem).width() - $(".nav-slider").width();
                        scrollCorrection = "+=" + scrollDelta + "px";

                        $(".nav-slider").scrollTo(scrollCorrection, 200, {
                            onAfter: function () {
                                getCurrentNavigationElements();
                            }
                        });
                    });

                    megamenuPrev.click(function (e) {

                        e.preventDefault();

                        $(".nav-slider").scrollTo(firstVisibleElem, 200, {
                            offset: { left: -40 },
                            onAfter: function () {
                                getCurrentNavigationElements();
                            }
                        });
                    });

                    // show scroll buttons when menu items don't fit into screen
                    $(window).resize(function () {

                        var liWidth = 0;
                        navElems.each(function () {
                            liWidth += $(this).width();
                        });

                        if (liWidth > megamenuContainer.width()) {
                            megamenuContainer.addClass("show-scroll-buttons");
                        }
                        else {
                            megamenuContainer.removeClass("show-scroll-buttons");
                        }

                        getCurrentNavigationElements();

                    }).trigger('resize');

                    function getCurrentNavigationElements() {
                        firstVisibleElem = null;
                        var p = $(".nav-slider", megamenuContainer);

                        navElems.each(function (i, val) {

                            var el = $(val);

                            if ((el.offset().left > p.offset().left) && firstVisibleElem == null) {
                                firstVisibleElem = el.prev();

                                if (firstVisibleElem.position().left == 0)
                                    isFirstItemVisible = true;
                                else
                                    isFirstItemVisible = false;
                            }

                            // if visible
                            if (el.offset().left + el.width() > p.offset().left + p.width()) {

                                lastVisibleElem = el;

                                if (parseInt(el.offset().left) + parseInt(el.width()) == parseInt(p.offset().left) + parseInt(p.width()))
                                    isLastItemVisible = true;
                                else
                                    isLastItemVisible = false;

                                // we've got everything we need, so get out of here
                                return false;
                            }
                        });

                        // show or hide navigation buttons depending on whether first or last navitems are displayed
                        if (!isFirstItemVisible) {
                            megamenu.addClass("megamenu--blend-prev");
                        }
                        else {
                            megamenu.removeClass("megamenu--blend-prev");
                        }

                        if (!isLastItemVisible) {
                            megamenu.addClass("megamenu--blend-next");
                        }
                        else {
                            megamenu.removeClass("megamenu--blend-next");
                        }
                    }
                });

                function initRotator(containerId) {

                    var container = $(containerId);
                    var catId = container.data("entity-id");
                    var displayRotator = container.data("display-rotator");

                    if ($(".pl-slider", container).length == 0 && catId != null && displayRotator) {

                        var rotatorColumn = $(".rotator-" + catId);

                        // init throbber
                        rotatorColumn.find(".rotator-content").throbber({ white: true, small: true, message: '' });

                        // wait a little to imply hard work is going on ;-)
                        setTimeout(function () {

                            $.ajax({
                                cache: false,
                                type: "POST",
                                url: settings.productRotatorAjaxUrl,
                                data: { "catId": catId },
                                success: function (data) {

                                    // add html view
                                    rotatorColumn.find(".rotator-content").html(data);

                                    // init scrolling
                                    var scrollableProductList = $(".mega-menu-product-rotator.scroll", container);
                                    var plSlider = $(".pl-slider", container);

                                    scrollableProductList.productListScroller({
                                        interval: settings.productRotatorInterval,
                                        cycle: settings.productRotatorCycle,
                                        duration: settings.productRotatorDuration
                                    });

                                    // add buttons
                                    container.find(".sb").scrollButton({
                                        nearSize: 36,
                                        farSize: "50%",
                                        target: $(".pl-slider", container),
                                        showButtonAlways: true,
                                        autoPosition: true,
                                        position: "inside",
                                        offset: -100,
                                        handleCorners: true,
                                        smallIcons: true,
                                        isBtnGroup: true,
                                        btnType: "primary"
                                    });

                                    // set article item width (important for mobile devices)
                                    $(".item-box", rotatorColumn).css({ "min-width": rotatorColumn.width(), "max-width": rotatorColumn.width() });

                                    // and now its hammertime
                                    var hammertime = new Hammer(scrollableProductList[0]);
                                    hammertime.add(new Hammer.Pan({ direction: Hammer.DIRECTION_HORIZONTAL, threshold: 80, pointers: 1 }));

                                    hammertime.on('panend', function (ev) {
                                        if (ev.direction == Hammer.DIRECTION_LEFT) { plSlider.trigger('next'); }
                                        if (ev.direction == Hammer.DIRECTION_RIGHT) { plSlider.trigger('prev'); }
                                    });
                                }
                            });
                        }, 1000);
                    }
                    else {
                        container.find(".placeholder").addClass("empty");
                    }
                }
            })
        }
    });
})(jQuery);