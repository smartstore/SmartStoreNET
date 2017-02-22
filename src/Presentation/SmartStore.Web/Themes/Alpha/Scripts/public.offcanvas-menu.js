;

/*
** OffCanvasMenu
*/

var AjaxMenu = (function ($, window, document, undefined) {

    var isInitialised = false;
    var viewport = ResponsiveBootstrapToolkit;
    var selectedMenuItemId = 0;

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

        // listen to clicks inside of #offcanvas-menu
        $("#offcanvas-menu").on('click', '.nav-item', function (e) {
            
            var item = $(this);
            var entityId = item.data("entity-id");
            var isAjaxNavigation = item.data("is-ajax-navigation");

            if (isAjaxNavigation == false) {
                // let event bubble up
                return true;
            }
            
            e.preventDefault();

            navigateToMenuItem(entityId);

            // for stopping event propagation
            return false;
        });
	});

	function navigateToMenuItem(entityId) {
	    $.ajax({
	        cache: false,
	        //url: '@(Url.Action("ChildCategories", "Catalog"))',
	        url: 'Catalog/ChildCategories',
	        data: { "categoryId": entityId },
	        type: 'POST',
	        success: function (response) {
	            // replace current cat container content with response
	            $("#offcanvas-menu-catalog").html(response);
	        },
	        error: function (jqXHR, textStatus, errorThrown) {
	            console.log(errorThrown);
	        },
	        complete: function () { }
	    });
	}

	return {

	    initMenu: function () {
	        //var offcanvasMenu = $('<aside id="offcanvas-menu" class="offcanvas x-offcanvas-lg offcanvas-overlay offcanvas-shadow" data-overlay="true" data-noscroll="true" data-blocker="true"><div class="offcanvas-content"></div></aside>')
            //    .appendTo('body');

	        var offcanvasMenu = $('#offcanvas-menu');

	        var menuContent = $(".menubar-section .menubar");
	        menuContent.clone().appendTo(offcanvasMenu.children().first());

            // cosmetics: remove classes and restructure given HTML of header menu
	        offcanvasMenu.find(".menubar-group").removeClass("pull-left");

	        /*
            var categories = $(".megamenu .navbar-nav");
	        categories
                .clone()
                .appendTo(offcanvasMenu.children().first());

	        $(".navbar-nav", offcanvasMenu).wrap("<div id='offcanvas-menu-catalog'></div>")
            */

	        offcanvasMenu.children().first().append("<div id='offcanvas-menu-catalog'><ul class='navbar-nav'></ul></div>");

	        //$("#shopbar-menu").click(function () {
	        //    offcanvasMenu.offcanvas('show');
	        //});

	        //selectedMenuItemId = categories.data("selected-menu-item");
	        selectedMenuItemId = $(".megamenu .navbar-nav").data("selected-menu-item");
	        navigateToMenuItem(selectedMenuItemId);

            /*
	        if (selectedMenuItemId != 0)
	        {
	            navigateToMenuItem(selectedMenuItemId);
	        }
            */

	        isInitialised = true;
		}
	}

})(jQuery, this, document);