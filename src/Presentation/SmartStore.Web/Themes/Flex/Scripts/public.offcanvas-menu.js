;

/*
** OffCanvasMenu
*/

var AjaxMenu = (function ($, window, document, undefined) {

    var isInitialised = false;
    var viewport = ResponsiveBootstrapToolkit;
    var selectedMenuItemId = 0;
    var menu = $("#offcanvas-menu #menu-container");

    $(function () {

        // if viewport <lg 
        if (viewport.is('<lg')) {
            AjaxMenu.initMenu();
        }
        
        // listen to viewport changed event and init if appropriate
        $(window).resize(
            viewport.changed(function () {
                if (viewport.is('<lg') && !isInitialised) {
                    AjaxMenu.initMenu();
                }
            })
        );

        // tab click events
        menu.on('click', '.nav-item', function (e) {
            
            var item = $(this);

            if (item.find(".nav-link").is("#manufacturer-tab")) {
                navigateToManufacturer();
            }
            else if (item.find(".nav-link").is("#service-tab")) {
                navigateToService();
            }
            else if (item.find(".nav-link").is("#category-tab")) {
                navigateToHomeLayer(false);
            }

            return false;
        });

        // menu click events
        menu.on('click', '.ocm-item', function (e) {
            
            var item = $(this);
            var entityId = item.data("id");
            var isAjaxNavigation = item.data("ajax");

            if (isAjaxNavigation == false) {
                // let event bubble up, so normal navigation via href attribute takes effect
                return true;
            }

            if (item.hasClass("animated")) {
                // prevent double clicks
                return false;
            }

            item.addClass("animated");
            
            e.preventDefault();

            navigateToMenuItem(entityId ? entityId : 0, item.hasClass("navigate-back") ? "left" : "right");
            
            // for stopping event propagation
            return false;
        });
	});

    function navigateToMenuItem(entityId, direction) {

        var categoryContainer = menu.find(".category-container");
        var firstCall = categoryContainer.length == 0;
        var categoryTab = entityId != 0 ? menu : menu.find("#ocm-categories");

        var currentLayer = $(".layer.in", menu);
        var nextLayer = currentLayer.next();
        var prevLayer = currentLayer.prev();

        if (direction == "left") {

            // check whether a previous layer exists (if it exists, its always the right one to navigate to)
            if (prevLayer.length > 0) {
                // special treatment when navigating back to home layer
                var isHome = prevLayer.hasClass("ocm-home-layer");

                if (isHome) {
                    prevLayer
                        .find(".ocm-nav-layer")
                        .removeClass("offcanvas-scrollable ocm-nav-layer layer");
                }
                
                currentLayer.removeClass("in");
                prevLayer.addClass("in");
                return;
            }

            // if no previous layer exists, make ajax call and prepend response
        }
        else if (direction == "right") {
            
            // check whether a next layer exists and if it has the same id as the element to which the user is navigating to
            if (nextLayer.data("id") == entityId) {
                currentLayer.removeClass("in");
                nextLayer.addClass("in");
                return;
            }
            else {
                // the layer to navigate to doesn't exist, so we remove all subsequent layers to build a new clean chain
                currentLayer.nextAll().remove();
            }
        }

	    $.ajax({
	        cache: false,
	        url: menu.data("url-item"),
	        data: { "categoryId": entityId },
	        type: 'POST',
	        success: function (response) {

	            // replace current menu content with response 
	            if (firstCall) {
	                if (entityId != 0)
	                    categoryTab.append(wrapAjaxResponse(response, " in", entityId));
	                else {
	                    categoryTab.append(response);
	                }
	            }
	            else {

	                var categoryContainerSlideIn;
	                var categoryContainerSlideOut = currentLayer;

	                if (direction == "left") {
                        
	                    if (entityId == 0) {
	                        navigateToHomeLayer(true);
	                        return;
	                    }
	                    
	                    categoryContainerSlideIn = $(wrapAjaxResponse(response, "", entityId)).prependTo(categoryTab);
	                }
	                else {
	                    categoryContainerSlideIn = $(wrapAjaxResponse(response, "", entityId)).appendTo(categoryTab);
	                }
	                
	                _.delay(function () {
	                    categoryContainerSlideIn.addClass("in");
                        
	                    if (direction !== undefined)
	                        categoryContainerSlideOut.removeClass("in");
                            
	                }, 100);

	                if (direction == undefined) {
	                    categoryContainerSlideIn = $(".ocm-home-layer");
	                    categoryContainerSlideOut = nextLayer;

	                    categoryContainerSlideIn
                            .addClass("in")
                            .find(".ocm-nav-layer")
                            .removeClass("offcanvas-scrollable ocm-nav-layer layer");

	                    categoryContainerSlideOut.removeClass("in");
	                }

	                categoryContainerSlideIn.on(Prefixer.event.transitionEnd, function (e) {
	                    categoryContainerSlideOut.find(".ocm-item").removeClass("animated");
	                    categoryContainerSlideIn.find(".ocm-item").removeClass("animated");
	                });
	            }
	        },
	        error: function (jqXHR, textStatus, errorThrown) {
	        	console.log(errorThrown);
	        },
	        complete: function () { }
	    });
	}

    function navigateToHomeLayer(isBackward) {

	    $.ajax({
	        cache: false,
	        url: menu.data("url-home"),
	        type: 'POST',
	        success: function (response) {

	            if (isBackward)
	                response = response.replace("ocm-home-layer layer in", "ocm-home-layer layer");

	            menu.prepend(response);
	            
	            menu.find("#category-tab").tab('show');

	            navigateToMenuItem(0);
	            AjaxMenu.initFooter();
	        },
	        error: function (jqXHR, textStatus, errorThrown) {
	            console.log(errorThrown);
	        },
	        complete: function () { }
	    });
	}

    function navigateToManufacturer() {

        var tabContent = menu.find("#manufacturer-tab");
        var manuTab = $("#ocm-manufacturers");
        var isInitialized = tabContent.data("initialized");

        if (isInitialized) {
            tabContent.tab('show');
            return;
        }

        $.ajax({
            cache: false,
            url: menu.data("url-manufacturer"),
            type: 'POST',
            success: function (response) {
                manuTab.html(response);
                tabContent.tab('show');
                tabContent.data("initialized", true);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
            },
            complete: function () { }
        });
    }

    function navigateToService() {

        var menuContent = $(".menubar-section .menubar").clone();
        var tabContent = menu.find("#service-tab");
        var serviceTab = $("#ocm-service");
        var isInitialized = tabContent.data("initialized");
        
        if (isInitialized) {
            tabContent.tab('show');
            return;
        }

        // hide currency & language selectors 
        menuContent.find(".currency-selector, .language-selector").addClass("hidden-xs-up");

        var myAccount = menuContent.find("#menubar-my-account");

        // open MyAccount dropdown initially
        myAccount.find(".dropdown").addClass("open");

        // place MyAccount menu on top
        menuContent.prepend(myAccount);

        menuContent.find(".dropdown-item").one("click", function (e) {
            e.stopPropagation();
        });

        serviceTab.html(menuContent);
        tabContent.data("initialized", true);
        tabContent.tab('show');

        return;
    }

    function wrapAjaxResponse(response, addClasses, id) {
        var responseHtml = "";

        responseHtml += '<div class="ocm-nav-layer layer offcanvas-scrollable ' + addClasses + '" data-id="' + id + '">';
        responseHtml += response;
        responseHtml += '</div>';

        return responseHtml;
    }

	return {

	    initMenu: function () {

	        var selectedMenuItemId = $(".megamenu .navbar-nav").data("selected-menu-item");
	        
	        if (selectedMenuItemId == 0) {
	            navigateToHomeLayer(false);
	        }
	        else {
	            navigateToMenuItem(selectedMenuItemId);
	        }

	        isInitialised = true;
	        return;
	    },

	    initFooter: function () {

	        var footer = $(".offcanvas-menu-footer");
	        var languageSelector = $(".menubar-section .language-selector");
	        var currencySelector = $(".menubar-section .currency-selector");
	        var ocmLanguageSelector = $("#ocm-language-selector", footer);
	        var ocmCurrencySelector = $("#ocm-currency-selector", footer);
	        var selectTitleLanguage = languageSelector.find(".dropdown-item[data-selected='true']").data("abbreviation");
	        var selectTitleCurrency = $(".menubar-link > span", currencySelector).text();
	        var displayCurrencySelector = currencySelector.length > 0;
	        var displayLanguageSelector = languageSelector.length > 0;
	        var languageOptions = "";
	        var currencyOptions = "";

	        if (!displayCurrencySelector && !displayCurrencySelector)
	            return;
	        else
	            footer.removeClass("hidden-xs-up");
	        
	        if (displayCurrencySelector) {
	            ocmCurrencySelector.parent().removeClass("hidden-xs-up");

	            $(currencySelector).find(".dropdown-item").each(function () {
	                var link = $(this);
	                var selected = link.data("selected") ? ' selected="selected" ' : '';
	                currencyOptions += '<option value="' + link.attr("href") + '"' + selected + '>' + link.data("abbreviation") + '</option>';
	            });

	            $("span", ocmCurrencySelector).text(selectTitleCurrency);
	            $(".form-control", ocmCurrencySelector).append(currencyOptions);
	        }

	        if (displayLanguageSelector) {
	            ocmLanguageSelector.parent().removeClass("hidden-xs-up");

	            $(languageSelector).find(".dropdown-item").each(function () {
	                var link = $(this);
	                var selected = link.data("selected") ? ' selected="selected" ' : '';
	                languageOptions += '<option value="' + link.attr("href") + '"' + selected + '>' + link.data("abbreviation") + '</option>';
	            });

	            $("span", ocmLanguageSelector).text(selectTitleLanguage);
	            $(".form-control", ocmLanguageSelector).append(languageOptions);
	        }

            // on change navigate to value 
	        $(footer).find(".form-control").on("change", function (e) {
	            var select = $(this);
	            window.setLocation(select.val());
	        });
	    }
	}
})(jQuery, this, document);