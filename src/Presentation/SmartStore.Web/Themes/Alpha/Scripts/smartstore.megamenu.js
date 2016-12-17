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
                var isSimple = megamenu.hasClass("simple");
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
                    if (navElems.hasClass('active') || megamenuDropdownContainer.hasClass('open')) {
                        tempLink = link;
                        openTimeout = setTimeout(function () { tryOpen(link); }, 50);
                    }
                    else {
                    	clearTimeout(openTimeout);

                    	$(link.data("target")).addClass("open");

                    	if (link.hasClass("dropdown-toggle")) {
                    		link.closest("li").addClass("active");
                    	}

                    	initRotator(link.data("target"));

                    	zoomContainer.css("z-index", "0");          	
                    }
                }

                if ($("html").hasClass("touch")) {

                    // Handle opening events for touch devices

                    megamenuContainer.on('clickoutside', function (e) {
                        closeNow($(".nav-item.active .nav-link"));
                    });

                    navElems.on("click", function (e) {
                        var link = $(this).find(".nav-link");
                        var opendMenuSelector = $(".nav-item.active .nav-link").data("target");

                        if (opendMenuSelector != link.data("target")) {
                            e.preventDefault();
                            closeNow($(".nav-item.active .nav-link"));
                            tryOpen(link);
                        }
                    });
                }
                else {

                    // Handle opening events for desktop workstations

                    $(".dropdown-menu", megamenuContainer).on('mouseenter', function (e) {
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
                        var openedMenuSelector = $(".nav-item.active .nav-link").data("target");

                        if ($(this).hasClass("nav-item")) {
                            closeNow($(".nav-item.active .nav-link"));
                            
                        }
                        else if (openedMenuSelector == link.data("target")) {
                            clearTimeout(closingTimeout);
                            return;
                        }

                        tryOpen(link);
                    })
                    .on("mouseleave", function () {
                        var link = $(this).find(".nav-link");
                        closeMenuHandler(link);
                    });
                }

                // correct dropdown position
                if (isSimple) {

                    var event = $("html").hasClass("touch") ? "click" : "mouseenter";

                    navElems.on(event, function (e) {
                        var navItem = $(this);
                        //var opendMenu = $(".dropdown-menu", $(".nav-item.active .nav-link").data("target"));
                        var opendMenu = $($(".nav-item.active .nav-link").data("target")).find(".dropdown-menu");

                        var offsetLeft = navItem.offset().left - megamenu.offset().left;

                        if (offsetLeft < 0) {
                            offsetLeft = 0;
                        }
                        else if (offsetLeft + opendMenu.width() > megamenu.width()) {
                            offsetLeft = megamenu.width() - opendMenu.width();
                        }

                        opendMenu.css("left", offsetLeft);
                    });
                }

                megamenuContainer.evenIfHidden(function (el) {
                    var scrollCorrection = null;
                    var lastVisibleElem = null;
                    var firstVisibleElem = null;
                    var isFirstItemVisible = true;
                    var isLastItemVisible = false;

                    megamenuContainer.find('ul').wrap('<div class="nav-slider" style="overflow: hidden;position: relative;" />');

                    // 
                    getCurrentNavigationElements();

                    var nav = $('.megamenu .nav');
                    var navSlider = $('.megamenu .nav-slider');
                    updateNavState();

                    megamenuNext.click(function (e) {
                        e.preventDefault();
                        scrollToNextInvisibleNavItem(false);
                    });

                    megamenuPrev.click(function (e) {
                        e.preventDefault();
                        scrollToNextInvisibleNavItem(true);
                    });

                    function scrollToNextInvisibleNavItem(backwards) {
                        // Ermittle den ersten komplett sichtbaren NavItem (von links oder rechts, je nach 'backwards')
                        var firstVisible = findFirstVisibleNavItem(backwards);

                        // Je nach 'backwards': nimm nächsten oder vorherigen Item (dieser ist ja unsichtbar und da soll jetzt hingescrollt werden)
                        var nextItem = backwards
							? firstVisible.prev()
							: firstVisible.next();

                        if (nextItem.length == 0)
                            return;

                        // Linke Pos des Items ermitteln
                        var leftPos = nextItem.position().left;

                        // 30 = offset für Pfeile
                        // Wenn 'backwards == true': zur linken Pos des Items scrollen
                        var newMarginLeft = (leftPos * -1) + 1 + 30;
                        if (!backwards) {
                            // Wenn 'backwards == true': zur rechten Pos des Items scrollen
                            var rightPos = leftPos + nextItem.outerWidth(true) + 1;
                            newMarginLeft = navSlider.width() - rightPos - (nextItem[0].nextElementSibling ? 30 : 0);
                        }

                        newMarginLeft = Math.min(0, newMarginLeft);

                        nav.css('margin-left', newMarginLeft + 'px').one("transitionend webkitTransitionEnd", function (e) {
                            // Führt UI-Aktualisierung NACH Anim-Ende durch (.one(trans...))
                            updateNavState();
                        });
                    }

                    function findFirstVisibleNavItem(fromLeft) {
                        var navItems = navElems;
                        if (!fromLeft) {
                            // NavItems umdrehen, da wir die Iteration von rechts starten
                            navItems = $($.makeArray(navElems).reverse());
                        }

                        var result;
                        var cntWidth = navSlider.width();
                        var curMarginLeft = parseFloat(nav.css('margin-left'));

                        function isInView(pos) {
                            var realPos = pos + curMarginLeft;
                            return realPos >= 0 && realPos < cntWidth;
                        }

                        navItems.each(function (i, el) {
                            // Durchläuft alle NavItems von links ODER rechts und steigt aus,
                            // sobald die linke UND rechte Kante des Items im sichtbaren Bereich liegen.
                            var navItem = $(el);
                            var leftPos = navItem.position().left;
                            var leftIn = isInView(leftPos);
                            if (leftIn) {
                                var rightIn = isInView(leftPos + navItem.outerWidth(true));
                                if (rightIn) {
                                    result = navItem;
                                    return false;
                                }
                            }
                        });

                        return result;
                    }

                    function updateNavState() {
                        // Aktualisiert Megamenü-Status: Arrows etc.
                        var realNavWidth = 0;
                        navElems.each(function (i, el) { realNavWidth += parseFloat($(this).outerWidth(true)); });

                        var curMarginLeft = parseFloat(nav.css('margin-left'));

                        if (realNavWidth > megamenu.width()) {
                            // NavItems passen nicht in Megamnü-Container: NextArrow anzeigen.
                            megamenu.addClass('megamenu-blend--next');
                        }

                        if (curMarginLeft < 0) {
                            // Es wurde nach rechts gescrollt: PrevArrow anzeigen.
                            megamenu.addClass('megamenu-blend--prev');

                            // Ermitteln, ob wir am Ende sind
                            var endReached = nav.width() >= realNavWidth;
                            if (endReached)
                                megamenu.removeClass('megamenu-blend--next')
                            else
                                megamenu.addClass('megamenu-blend--next');
                        }
                        else {
                            // Wir sind am Anfang: PrevArrow ausblenden.
                            megamenu.removeClass('megamenu-blend--prev');
                        }
                    }

                    //oh, oh, oh, oh, can't touch this ;-/
                    var hammertime = new Hammer($(".megamenu")[0]);
                    hammertime.add(new Hammer.Pan({ direction: Hammer.DIRECTION_HORIZONTAL }));

                    if (isSimple) {
                        hammertime.on('panstart', function (ev) {

                            closeNow($(".nav-item.active .nav-link"));

                            /*
                            var link = $(".nav-item.active .nav-link");

                            $(link.data("target")).removeClass("open");

                            if (link.hasClass("dropdown-toggle")) {
                                link.closest("li").removeClass("active");
                            }
                            */
                        });
                    }

                    hammertime.on('panend', function (ev) {
                        getCurrentNavigationElements();
                        closeNow($(".nav-item.active .nav-link"));

                        if (ev.direction == Hammer.DIRECTION_LEFT) {megamenu.addClass("megamenu-blend--prev");}
                        if (ev.direction == Hammer.DIRECTION_RIGHT) {megamenu.addClass("megamenu-blend--next");}
                    });

                    function onPageResized() {
                    	updateNavState();

                    	var liWidth = 0;
                    	navElems.each(function () { liWidth += $(this).width(); });

                    	if (liWidth > megamenuContainer.width()) {
                    		megamenuContainer.addClass("show-scroll-buttons");
                    	}
                    	else {
                    		megamenuContainer.removeClass("show-scroll-buttons");
                    	}

                    	getCurrentNavigationElements();
                    }

                	// show scroll buttons when menu items don't fit into screen
                    EventBroker.subscribe("page.resized", function (msg, viewport) {
                    	onPageResized();
                    });
                    onPageResized();

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
                            megamenu.addClass("megamenu-blend--prev");
                        }
                        else {
                            megamenu.removeClass("megamenu-blend--prev");
                        }

                        if (!isLastItemVisible) {
                            megamenu.addClass("megamenu-blend--next");
                        }
                        else {
                            megamenu.removeClass("megamenu-blend--next");
                        }
                    }
                });

                function initRotator(containerId) {

                    var container = $(containerId);
                    var catId = container.data("entity-id");
                    var displayRotator = container.data("display-rotator");

                    //if ($(".pl-slider", container).length == 0 && catId != null && displayRotator) {
                    if (catId != null && displayRotator) {

                        var rotatorColumn = $(".rotator-" + catId);

                        // init throbber
                        //rotatorColumn.find(".rotator-content").throbber({ white: true, small: true, message: '' });

                        // clear content & init throbber
                        rotatorColumn.find(".rotator-content")
							.html('<div class="placeholder"></div>')
							.throbber({ white: true, small: true, message: '' });

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
                                        autoPosition: false,
                                        position: "inside",
                                        offset: -100,
                                        handleCorners: true,
                                        smallIcons: true,
                                        btnType: "primary",
                                        opacityOnHover: false
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

                                    if (container.hasClass("open")) {
                                        container.data("display-rotator", false);
                                    }
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