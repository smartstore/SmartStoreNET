(function($) {
	
    $.fn.extend({
        productListScroller: function(settings) {
            
            var defaults = {
            	preloadElements:		0		// amount of products that get pre loaded
            };
            
            var settings = $.extend(defaults, settings);
			
            return this.each(function() {
            	
            	var list = $(this);
            	
            	list.evenIfHidden(function(el) {
            		
            	    var visibleElemnts = parseInt(list.outerWidth() / list.find(".item-box:first").outerWidth());
					visibleElemnts = (visibleElemnts >= 1) ? 1 : (visibleElemnts - 1);
					
					list.find('.pl-row').wrap('<div class="pl-slider" style="overflow: hidden;position: relative;" />') ;
					
					// init serial scroll
					list.serialScroll({
				        target:		'.pl-slider',
				        items:		'.item-box',
				        prev: 		'.pl-scroll-prev',
				        next: 		'.pl-scroll-next',
				        axis:		'x',
				        duration:	300,
				        easing:		'easeInOutQuad',
				        force:		true,
				        cycle: 		false,
				        exclude:    visibleElemnts -1,
				        onBefore:	function( e, elem, $pane, $items, pos ){
				        	
				        	var plList = $pane.parent(),
				        		//lastOptimized = plList.data('last-opt'),
				        		isFirst = (pos == 0),
				        		isLast = ((pos + visibleElemnts) == $items.length);
				        	
				        	if (isFirst)
				        		plList.find('.pl-scroll-prev').data("ScrollButton").enable(false);
				        	else
				        		plList.find('.pl-scroll-prev').data("ScrollButton").enable(true);
				        	
				        	if (isLast)
				        		plList.find('.pl-scroll-next').data("ScrollButton").enable(false);
				        	else
				        		plList.find('.pl-scroll-next').data("ScrollButton").enable(true);
				        	
				        	//if (lastOptimized != 0) {
					        	//plList.productListOptimizer.adjustElementRange( plList , 1 );
				        	//}
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