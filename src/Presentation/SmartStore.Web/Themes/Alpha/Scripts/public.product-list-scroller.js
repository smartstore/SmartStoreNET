(function($) {
	
    $.fn.extend({
        productListScroller: function(settings) {
            
            var defaults = {
                preloadElements:    0,		// amount of products that get pre loaded
                interval:           0,
                cycle:              false,
                duration:           300
            };
            
            var settings = $.extend(defaults, settings);
			
            return this.each(function() {
            	
            	var list = $(this);
            	
            	list.evenIfHidden(function(el) {
            		
            	    var visibleElemnts = parseInt(list.outerWidth() / list.find(".item-box:first").outerWidth());

            	    //visibleElemnts = (visibleElemnts >= 1) ? 1 : (visibleElemnts - 1);
            	    visibleElemnts = (visibleElemnts >= 1) ? (visibleElemnts - 2) : 0;
				    
					list.find('.pl-row').wrap('<div class="pl-slider" style="overflow: hidden;position: relative;" />') ;
					
					// init serial scroll
					list.serialScroll({
				        target:		'.pl-slider',
				        items:		'.item-box',
				        prev: 		'.pl-scroll-prev',
				        next: 		'.pl-scroll-next',
				        axis:		'x',
				        duration:   settings.duration,
				        easing:		'easeInOutQuad',
				        force:		true,
				        cycle:      settings.cycle,
				        interval:   settings.interval,
				        exclude:    visibleElemnts,
				        onBefore:	function( e, elem, $pane, $items, pos ) {
				        	
				        	var plList = $pane.parent(),
				        		isFirst = (pos == 0),
				        		isLast = ((pos + visibleElemnts) == $items.length - 1);

				        	var btnPrev = plList.find('.pl-scroll-prev');
				        	var btnNext = plList.find('.pl-scroll-next');

				        	if (isFirst)
				        	    btnPrev.data("ScrollButton").enable(false);
				        	else
				        	    btnPrev.data("ScrollButton").enable(true);
				        	
				        	if (isLast)
				        	    btnNext.data("ScrollButton").enable(false);
				        	else
				        	    btnNext.data("ScrollButton").enable(true);

				        	btnPrev.blur();
				        	btnNext.blur();
				        }
					});
					
					$(window).load(function() {
					//$.preload(list.find("img"), function() {
						
					    var itemsWidth = list.find('.item-box:first').outerWidth(true) * list.find('.item-box').length;

						if(itemsWidth > list.width()){
							list.find(".sb").scrollButton({ 
								nearSize: 96,
								farSize: 28,
								target: list, 
								showButtonAlways: true,
								autoPosition: true,
								position: "inside",
								offset: 0,
								handleCorners: true,
								smallIcons: false,
								hostFix: true/*,
								click: function(dir) {
									var el = $(this);
									var btn = el.data("ScrollButton");
								}*/ 
							});
						}
					});
				});
            });
        }
    });
})(jQuery);