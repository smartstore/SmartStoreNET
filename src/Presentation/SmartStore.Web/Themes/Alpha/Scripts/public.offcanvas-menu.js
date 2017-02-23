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
                // let event bubble up, so normal navigation via href attribute takes effect
                return true;
            }
            
            e.preventDefault();

            navigateToMenuItem(entityId, item.hasClass("back-to-parent-cat") ? "right" : "left");

            // for stopping event propagation
            return false;
        });
	});

	function navigateToMenuItem(entityId, direction) {
	    $.ajax({
	        cache: false,
	        //url: '@(Url.Action("ChildCategories", "Catalog"))',
	        url: 'Catalog/ChildCategories',
	        data: { "categoryId": entityId },
	        type: 'POST',
	        success: function (response) {

	            // replace current cat container content with response

	            var categoryContainer = $(".category-container:first");
	            var firstCall = categoryContainer.length == 0;
	            
	            if (firstCall) {
	                $("#offcanvas-menu-catalog").append(response);
	            }
	            else {

	                var responseHtml = response.replace("category-container", "category-container category-container-slide-in category-container-slide-in--from-" + direction);
	                
	                $("#offcanvas-menu-catalog").append(responseHtml);
	                
	                var menuWidth = $("#offcanvas-menu-catalog").width();
	                var categoryContainerSlideIn = $(".category-container-slide-in");

	                categoryContainerSlideIn.css("width", menuWidth + "px");

	                if (direction == "left") {
	                    categoryContainerSlideIn.css("left", menuWidth + "px");
	                    categoryContainer.css("margin-left", "-" + menuWidth + "px");
	                    categoryContainerSlideIn.css("left", "0px");
	                }
	                else {
	                    //categoryContainerSlideIn.css("right", "-" + menuWidth + "px");
	                    categoryContainer.css("margin-left", menuWidth + "px");
	                    categoryContainerSlideIn.css("right", "0px");
	                }

	                // remove slid container after transition
	                _.delay(function () {

	                    categoryContainer.remove();

	                    // finaly remove temporary class
	                    categoryContainerSlideIn.removeClass("category-container-slide-in");

	                }, 600);
	            }
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

	        var ownMenu = '<div class="button-bar">';
	        ownMenu += '<a class="back-to-home m-r-05" href="@Url.RouteUrl("HomePage")"><i class="fa fa-home"></i>Home</a>';
	        ownMenu += '<a class="close-offcanvas-menu offcanvas-closer"><i class="fa fa-times"></i>Schließen</a>';
	        ownMenu += '</div>';

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

	        isInitialised = true;
		}
	}

})(jQuery, this, document);