;

/*
** OffCanvasMenu
*/

var AjaxMenu = (function ($, window, document, undefined) {

    var isInitialised = false;
    var viewport = ResponsiveBootstrapToolkit;
    var selectedEntityId = 0;
    var currentCategoryId = 0;
    var currentProductId = 0;
	var currentManufacturerId = 0;
	var publicStoreNavigationAllowed = true;

    var menu = $("#offcanvas-menu #menu-container");

    $(function () {

        // if viewport <lg 
        if (viewport.is('<lg')) {
            AjaxMenu.initMenu();
        }

    	// listen to viewport changed event and init if appropriate
        EventBroker.subscribe("page.resized", function (msg, viewport) {
        	if (viewport.is('<lg') && !isInitialised) {
        		AjaxMenu.initMenu();
        	}
        });

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
			e.preventDefault();

            var item = $(this);
            var categoryId = item.data("id");
            var isAjaxNavigation = item.data("ajax");

            if (!isAjaxNavigation) {
                window.setLocation(item.find(".ocm-link").attr("href"));
                return true;
            }

            if (item.hasClass("animated")) {
                // prevent double clicks
                return false;
            }

            item.addClass("animated");                    
			
            navigateToMenuItem(categoryId || 0, item.hasClass("navigate-back") ? "out" : "in");
            
            // stop event propagation
            return false;
        });
	});

    function navigateToMenuItem(categoryId, direction) {
		var cnt = menu.find(".category-container"),
			firstCall = cnt.length == 0,
			categoryTab = categoryId != 0 ? menu : menu.find("#ocm-categories"),
			currentLayer = $(".layer.show", menu),
			nextLayer = currentLayer.next(),
			prevLayer = currentLayer.prev();

		if (direction === "out") {
            // check whether a previous layer exists (if it exists, its always the right one to navigate to)
            if (prevLayer.length > 0) {
                // special treatment when navigating back to home layer
                var isHome = prevLayer.hasClass("ocm-home-layer");
                if (isHome) {
                    prevLayer
                        .find(".ocm-nav-layer")
                        .removeClass("offcanvas-scrollable ocm-nav-layer layer");
                }
                
                currentLayer.removeClass("show");
                prevLayer.addClass("show");
                return;
            }

            // if no previous layer exists, make ajax call and prepend response
        }
		else if (direction === "in") {
            // check whether a next layer exists and if it has the same id as the element to which the user is navigating to
            if (nextLayer.data("id") == categoryId) {
                currentLayer.removeClass("show");
                nextLayer.addClass("show");
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
	        data: {
	            "categoryId": categoryId,
                "currentCategoryId": currentCategoryId,
                "currentProductId": currentProductId
	        },
	        type: 'POST',
			success: function (response) {
	            // replace current menu content with response 
	            if (firstCall) {
	                if (categoryId != 0)
	                    categoryTab.append(wrapAjaxResponse(response, " show", categoryId));
	                else {
	                    categoryTab.append(response);
	                }
	            }
	            else {
	                var cntSlideIn;
	                var cntSlideOut = currentLayer;

					if (direction === "out") {
						if (categoryId == 0) {
	                        navigateToHomeLayer(true);
	                        return;
	                    }
	                    
						cntSlideIn = $(wrapAjaxResponse(response, "", categoryId)).prependTo(categoryTab);
	                }
	                else {
						cntSlideIn = $(wrapAjaxResponse(response, "", categoryId)).appendTo(categoryTab);
	                }

	                _.delay(function () {
						cntSlideIn.addClass("show");
					
						if (direction)
							cntSlideOut.removeClass("show");    
	                }, 100);

	                if (!direction) {
						cntSlideIn = $(".ocm-home-layer");
						cntSlideOut = nextLayer;

						cntSlideIn
                            .addClass("show")
                            .find(".ocm-nav-layer")
                            .removeClass("offcanvas-scrollable ocm-nav-layer layer");

						cntSlideOut.removeClass("show");
	                }

					cntSlideIn.on(Prefixer.event.transitionEnd, function (e) {
						cntSlideOut.find(".ocm-item").removeClass("animated");
						cntSlideIn.find(".ocm-item").removeClass("animated");
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
        var tabContent = menu.find("#category-tab");

        if (tabContent.data("initialized")) {
            tabContent.tab('show');
            return;
        }
		
	    $.ajax({
	        cache: false,
	        url: menu.data("url-home"),
	        type: 'POST',
	        success: function (response) {
	            if (isBackward) {
	                response = response.replace("ocm-home-layer layer show", "ocm-home-layer layer");
	            }

				menu.prepend(response);

	            tabContent = menu.find("#category-tab");
	            tabContent.tab('show');
	            navigateToMenuItem(0);
	            AjaxMenu.initFooter();
				tabContent.data("initialized", true);
				
				if (publicStoreNavigationAllowed == false)
					navigateToService();
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
                manuTab.find("li[data-id='" + currentManufacturerId + "']").addClass("selected");
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
        menuContent.find(".currency-selector, .language-selector").addClass("d-none");

        // remove data-toggle attributes
        menuContent.find("[data-toggle=dropdown]").removeAttr("data-toggle");

        // open MyAccount dropdown initially
		var myAccount = menuContent.find("#menubar-my-account");
		myAccount.find(".dropdown").addClass("openend");
		myAccount.find(".dropdown-menu").addClass("show");

        // place MyAccount menu on top
        menuContent.prepend(myAccount);

        menuContent.find(".dropdown-item").one("click", function (e) {
            e.stopPropagation();
        });

        // handle dropdown opening
        serviceTab.on("click", ".dropdown > .menubar-link", function (e) {
			var dropdown = $(this).parent();
			var dropdownMenu = dropdown.find(".dropdown-menu");

			if (dropdownMenu.length == 0)
                return true;

            e.preventDefault();
			dropdown.toggleClass("openend");
			dropdownMenu.toggleClass("show");
            return false;
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
	        var nav = $(".megamenu .navbar-nav");
            selectedEntityId = nav.data("selected-entity-id") || 0;
	        currentCategoryId = nav.data("current-category-id");
	        currentProductId = nav.data("current-product-id");
	        currentManufacturerId = nav.data("current-manufacturer-id");
			publicStoreNavigationAllowed = menu.data("public-store-navigation-allowed");
			
            if (selectedEntityId == 0) {
				navigateToHomeLayer(false);
			}
            else {
                navigateToMenuItem(selectedEntityId);
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
	        var displayCurrencySelector = currencySelector.length > 0;
	        var displayLanguageSelector = languageSelector.length > 0;
	        var languageOptions = "";
	        var currencyOptions = "";

	        if (!displayCurrencySelector && !displayLanguageSelector)
	            return;
	        else
	            footer.removeClass("d-none");
	        
	        if (displayCurrencySelector) {
	            ocmCurrencySelector.removeClass("d-none");

	            $(currencySelector).find(".dropdown-item").each(function () {
	                var link = $(this);
	                var selected = link.data("selected") ? ' selected="selected" ' : '';
	                currencyOptions += '<option value="' + link.attr("href") + '"' + selected + '>' + link.data("abbreviation") + '</option>';
	            });

	            $(".form-control", ocmCurrencySelector).append(currencyOptions);
	        }

	        if (displayLanguageSelector) {
	            ocmLanguageSelector.removeClass("d-none");

	            $(languageSelector).find(".dropdown-item").each(function () {
	                var link = $(this);
	                var selected = link.data("selected") ? ' selected="selected" ' : '';
	                languageOptions += '<option value="' + link.attr("href") + '"' + selected + '>' + link.data("abbreviation") + '</option>';
	            });

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