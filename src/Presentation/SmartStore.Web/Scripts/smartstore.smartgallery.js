/*
 *  Project: smartstore.smartgallery
 *  Version: 1.0 
 *  Author: Murat Cakir
 */

;(function ( $, window, document, undefined ) {
    
    var pluginName = 'smartGallery';
    
	var defaultEasing = "easeInQuad";
	var animations = {
	    'slide': function (cnt, dir, desc) {
			var curLeft = parseInt(cnt.css('left'), 10);
			var oldImageLeft = 0;
			if (dir == 'left') {
				oldImageLeft = '-'+ this.imageWrapperWidth +'px';
				cnt.css('left', this.imageWrapperWidth +'px');
			} 
			else {
				oldImageLeft = this.imageWrapperWidth +'px';
				cnt.css('left', '-'+ this.imageWrapperWidth +'px');
			}
			if (desc) {
				desc.css('bottom', '-'+ desc[0].offsetHeight +'px');
				if (this.inWrapper) {
					desc.animate({bottom: 0}, this.options.animationSpeed * 2);
				}
			}
			if (this.currentDescription) {
				this.currentDescription.animate({bottom: '-'+ this.currentDescription[0].offsetHeight +'px'}, this.options.animationSpeed * 2);
			}
			return {oldImage: {left: oldImageLeft},
			        newImage: {left: curLeft}};	
		},
			
		'fade' : function(cnt, dir, desc) {
			cnt.css('opacity', 0);
			return {oldImage: {opacity: 0},
			        newImage: {opacity: 1},
			        easing: 'linear'};	
		},
			
		'none' : function(cnt, dir, desc) {
			cnt.css('opacity', 0);
			return {oldImage: {opacity: 0},
			        newImage: {opacity: 1},
			        speed: 0};
		}	
	}

	var defaultZoomOpts = {
	    zoomType: 'window',
	    cursor: 'pointer',
	    easing: true,
	    easingDuration: 1000,
	    borderSize: 1,
	    borderColour: "#999",
	    lensFadeIn: 400,
	    lensFadeOut: 400,

	    // zoomType 'lens' options
	    lensShape: "round",
	    lensSize: 150,
	    containLensZoom: false,

	    // zoomType 'window' options
	    zoomWindowFadeIn: 400,
	    zoomWindowFadeOut: 400,
	    zoomTintFadeIn: 400,
	    zoomTintFadeOut: 400,
	    zoomWindowOffetx: 10,
	    zoomWindowWidth: 400,
	    zoomWindowHeight: null // null to calculate
	};
	
	function SmartGallery( element, options ) {
		var self = this;

		this.element = element;
		var el = this.el = $(element);

		var meta = $.metadata ? $.metadata.get(element) : {};
		var opts = this.options = $.extend(true, {}, options, meta || {});
		
		this.init = function() {
			this.setupElements(); // DO
			if (opts.width) {
				this.imageWrapperWidth = opts.width;
				this.imageWrapper.width(opts.width);
				el.width(opts.width);
			} 
			else {
			    this.imageWrapperWidth = this.imageWrapper.width();
			}
			
			this.imageWrapper.height(opts.height || 400);
			this.imageWrapperHeight = opts.height;
			
			this.navDisplayWidth = this.nav.width();
			this.currentIndex = 0;
			
			this.findImages();
			
			if (this.options.box && this.options.box.enabled) {
				this.initBox();
			}
			else {
				this.imageWrapper.on("click", ".sg-image > a", function() {
					return self.fireCallback(self.options.callbacks.imageClick);
				});	
			}
			
			if (!opts.displayImage) {
				this.imageWrapper.hide(0);	
			}
			
			if (this.images.length > 1) {
				this.initThumbNav();
			}
			else {
				this.nav.hide(0);	
			}
			
			if(opts.enableKeyboardMove) {
				this.initKeyEvents();
			};
		
			//this.findImages();
			
			var startAt = parseInt(opts.startIndex, 10);
			if (window.location.hash && window.location.hash.indexOf('#sg-image') === 0) {
				startAt = window.location.hash.replace(/[^0-9]+/g, '');
				if(!_.isNumber(startAt)) {
					startAt = opts.startIndex;
				};
			};
			
			this.loading(true);
			
			this.showImage(startAt);
			
            /* // TODO: (MC) this is more challenging than i thought (!)
			if (opts.responsive && (!opts.width)) {
			    EventBroker.subscribe("page.resized", function (data) {
			        // recalc some metrics on page resize
			        self.imageWrapperWidth = self.imageWrapper.width();
			        self.navDisplayWidth = self.nav.width();

			        // center current image
			        var imgContainer = self.imageWrapper.find('.sg-image');
			        var img = imgContainer.find("img");
			        self._centerImage(imgContainer, img.width(), img.height());

			        // hide show/scroll buttons
                    // [...]
			    });
			}
            */

			/*this.fireCallback(opts.callbacks.init);*/

		};
		
		this.initialized = false;
		this.init();
		this.initialized = true;	
	}
	
	SmartGallery.prototype = {
		
		imageWrapper: null,
		//imageOverlay: null,
	    nav: null,
	    loader: null,
	    preloads: null,
	    thumbsWrapper: null,
	    box: null,
	    	
	    imageWrapperWidth: 0,
	    imageWrapperHeight: 0,
	    currentIndex: 0,
	    currentImage: null,
	    currentDescription: null,
	    navDisplayWidth: 0,
	    navListWidth: 0,
	    noScrollers: false,
	    images: null,
	    inTransition: false,
	    inWrapper: false,
	    	
	  	scrollForward: null,
	  	scrollBack: null,
	  		
	  	origHtml: null,
	    	
		setupElements: function() {
			var el = this.el;
			
			this.origHtml = this.el.outerHtml();
			this.imageWrapper = el.find('.sg-image-wrapper').empty();
			this.nav = el.find('.sg-nav').addClass("invisible");
			this.thumbsWrapper = this.nav.find('.sg-thumbs');
			this.preloads = $('<div class="sg-preloads"></div>');
			this.loader = $('<div class="ajax-loader-small sg-loader"></div>');
			this.imageWrapper.append(this.loader);
			this.loader.hide();
			$(document.body).append(this.preloads);
		},

		reset: function () {
		    var self = this;
		    if (self.preloads) {
		        self.preloads.remove();
		    }
		    var oldZoomImage = self.imageWrapper.find('.sg-image img').data("elevateZoom");
		    if (oldZoomImage) {
		        console.log(oldZoomImage);
		        oldZoomImage.reset();
		    }
		},

		loading: function(value) {
			if (value) {
				this.loader.show();
			} 
			else {
				this.loader.hide();
			};
		},

		addAnimation: function(name, fn) {
			if ($.isFunction(fn)) {
				animations[name] = fn;
			};
		},
		
		findImages: function() {
			var self = this;
			this.images = [];
			var thumbWrapperWidth = 0;
			var thumbsLoaded = 0;
			var thumbs = this.thumbsWrapper.find('a');
			var thumbCount = thumbs.length;
			
			thumbs.each( function(i) {
				var link = $(this);
				var imageSrc = link.attr('href');
				var thumb = link.find('img');
				// Check if the thumb has already been loaded
				if (!self.isImageLoaded(thumb[0])) {
					thumb.load(function() {
					    thumbWrapperWidth += this.parentNode.parentNode.offsetWidth;
					    thumbsLoaded++;
					});
				} 
				else{
					thumbWrapperWidth += thumb[0].parentNode.parentNode.offsetWidth;
					thumbsLoaded++;
				}
				
				var linkSize = self.options.thumbSize + 2;
				link.addClass('sg-thumb' + i)
			        .css({ width: linkSize + "px", height: linkSize + "px", lineHeight: (linkSize-2) + "px" });
				
				link.on({ 
				    'click': function () {
						var fn = function() {};
						if (!self.options.displayImage) {
							self._showBox(i);	
						}
						self.showImage(i, fn);
						return false;
					},
					'mouseenter': function() {
						self.preloadImage(i);
					}
				});
				
				var s;
				var href = false;
				
				s = thumb.data('sg-link') || thumb.attr('rel') || thumb.attr('longdesc');
				if (!s && self.options.ensureLink) {
					s = link.attr("href");	
				}
				if (s) href = s;
				
				s = "";
				var title = false;
				s = thumb.data('sg-title') || thumb.attr('title');
				if (s) title = s;
				thumb.removeAttr('title'); // das stört sonst
				
				s = "";
				var desc = false;
				s = thumb.data('sg-desc') || thumb.attr('alt');
				if (s && s != title) desc = s;
				
				self.images[i] = { thumb: thumb.attr('src'), 
				                   image: imageSrc,
                                   zoom: link.data("zoom-image"),
								   error: false,
				                   preloaded: false, 
				                   desc: desc, 
				                   title: title, 
				                   size: false,
				                   link: href
				};
			});
			
			// Wait until all thumbs are loaded, and then set the width of the ul
			var list = self.nav.find('.sg-thumb-list');
			var setULWidth = function() {
				
				if (thumbCount == thumbsLoaded) {
					var scrollers = self.nav.find('.scroll-button');
					if (thumbWrapperWidth <= self.navDisplayWidth) {
						// Liste passt in den Container.
						// ScrollButtons entfernen
						scrollers.each(function() {
							$(this).parent().remove();
						});
						// ...und verhindern, dass ein ThumbJump durchgeführt würde.
						self.noScrollers = true;
						// ...und Liste horizontal zentrieren
						list.css('left', (self.navDisplayWidth - thumbWrapperWidth) / 2 +'px');	
					}
					else {
						// Liste ist größer als Container!
						// wir brauchen ScrollButtons:	
						if (scrollers.length > 0) {
							self.nav.addClass("has-buttons"); // der hier hat die paddings
							var parentHeight = scrollers.parent().innerHeight();
							scrollers.height(parentHeight - scrollers.verticalCushioning());
						}
					}
					
					// tatsächliche Breite der Liste setzen
					//list.css('width', (thumbWrapperWidth) +'px');
					list.css('width', (thumbWrapperWidth + 1) +'px');
					
					clearInterval(inter);
					
					self.navListWidth = list.outerWidth(true);
					
					self.nav.removeClass("invisible");
					
				}
			}
			
			var inter = setInterval( setULWidth, 50 );
	
		},

		initKeyEvents: function() {
			var self = this;
			$(document).keydown(function(e) {
				if (e.keyCode == 39) {
					// right arrow
					self.nextImage();
				} 
				else if (e.keyCode == 37) {
					// left arrow
					self.prevImage();
				}
			});
		},

		initThumbNav: function() {
			var self = this;
			
			this.scrollForward = $('<a href="#" class="sb invisible sb-dir-right">></a>');
			this.scrollBack = $('<a href="#" class="sb invisible sb-dir-left"><</a>');
			this.nav.prepend(this.scrollForward.wrap('<div class="sg-scroll-forward"></div>').parent());
			this.nav.prepend(this.scrollBack.wrap('<div class="sg-scroll-back"></div>').parent());

			var hasScrolled = 0;
      		var thumbsScrollInterval = false;
			
			this.nav.find(".sb").scrollButton({ 
				nearSize: "100%",
				farSize: "100%",
				showButtonAlways: true,
				autoPosition: false,
				position: "outside",
				offset: 4,
				handleCorners: false,
				smallIcons: true,
				click: function(dir) {
					// We don't want to jump the whole width, since an image
					// might be cut at the edge
					var width = self.navDisplayWidth - 80;
					if (self.options.scrollJump >>> 0) {
						width = self.options.scrollJump;
					}
					if (dir == 'right') {
						var left = self.thumbsWrapper.scrollLeft() + width;
					} 
					else {
						var left = self.thumbsWrapper.scrollLeft() - width;
					}
					self.thumbsWrapper.animate({ scrollLeft: left +'px' }, 400, "easeOutQuad");
					self._toggleScrollButtons(left);
					return false;
				},
				enter: function(dir) { 
					thumbsScrollInterval = setInterval(function() {
						hasScrolled++;
						var left = self.thumbsWrapper.scrollLeft() + 1;
						if (dir == 'left') {
							left = self.thumbsWrapper.scrollLeft() - 1;
						};
						self.thumbsWrapper.scrollLeft(left);
						self._toggleScrollButtons(left);
					}, 10);	
				},
				leave: function(dir) { 
			          hasScrolled = 0;
			          clearInterval(thumbsScrollInterval);	
				}
			});
		},
		
		initBox: function() {
			var self = this;
			
			//this.box = $('<div class="sg-box"></div>').hide(0).appendTo(this.el);
			
			/*$.each(this.images, function(i, img) {
				self.box.append(
					'<a class="sg-box-item" href="{0}" title="{1}" rel="sg-box"></a>'.format( 
						img.link || img.image, 
						img.desc || img.title 
					)
				);
			});*/

			if (this.options.displayImage) {
			    this.imageWrapper.on("click", ".sg-image > a", function (event) {

			        event.preventDefault();
			        $('#modal-gallery').modal({
			            "target": "#modal-gallery",
			            "selector": ".sg-thumb-list li a",
			            "index": self.currentIndex
			        }).on('beforeOpen', function () {
			            var modalData = $(this).data('modal');
			            modalData.options.index = self.currentIndex;
			        });

				});
			}
		},
		
		refreshDetailImage: function(src) {
		    
			var self = this,
			 	imgWrapper = this.imageWrapper,
				image = imgWrapper.find('.sg-image img'),
				newImgWidth = "",
				newImgHeight = "";
			
			image
				.attr("src", src)
				.removeAttr("width")
           		.removeAttr("height");
			
			image.load(function(){
					var newImage = imgWrapper.find('.sg-image img');
					newImgWidth = newImage.width();
					newImgHeight = newImage.height();
					newImage.attr("width", newImgWidth).attr("height", newImgHeight);
				
					imgWrapper.find('.sg-image').css({
						'width': newImgWidth + 'px',
						'height': newImgHeight + 'px'
					});
					
					self._centerImage(imgWrapper.find('.sg-image'), newImgWidth, newImgHeight);
					
					if (imgWrapper.parent().attr('id') == 'pd-gallery-big') {
						imgWrapper.find('.sg-image a').attr('href', src);
					}
			});
		},
		
		_showBox: function (idx) {
		    idx = idx === undefined ? this.currentIndex : idx;
		    this.imageWrapper.find('.sg-image > a').click();
			var fn = this.options.callbacks.imageClick;
			return $.isFunction(fn) ? fn.call(this) : false;
		},
			
		_toggleScrollButtons: function(scrollLeft) {
			if (this.noScrollers) return;
			
			var fwd = this.scrollForward, back = this.scrollBack;
			var plugin, enabled = true;
			scrollLeft = scrollLeft !== undefined ? scrollLeft : this.thumbsWrapper.scrollLeft();
			var listWidth = this.navListWidth || this.nav.find('.sg-thumb-list').outerWidth(true);
			if (fwd) {	
				enabled = (this.navDisplayWidth - (listWidth - scrollLeft)) < 0;
				plugin = fwd.data("ScrollButton");
				plugin.enable(enabled);
			}
			if (back) {
				enabled = scrollLeft > 0;
				plugin = back.data("ScrollButton");
				plugin.enable(enabled);
			}	
		},

		/**
		 * Checks if the image is small enough to fit inside the container
		 * If it's not, shrink it proportionally
		 */
		_getContainedImageSize: function(imageWidth, imageHeight) {
			var ratio = 0;
			if (imageHeight > this.imageWrapperHeight) {
				ratio = imageWidth / imageHeight;
				imageHeight = this.imageWrapperHeight;
				imageWidth = this.imageWrapperHeight * ratio;
			};
			if (imageWidth > this.imageWrapperWidth) {
				ratio = imageHeight / imageWidth;
				imageWidth = this.imageWrapperWidth;
				imageHeight = this.imageWrapperWidth * ratio;
			};
			return {width: imageWidth, height: imageHeight};
		},

		/**
		 * If the image dimensions are smaller than the wrapper, we position
		 * it in the middle anyway
		 */
		_centerImage: function(imgContainer, imageWidth, imageHeight) {
			imgContainer.css('top', '0px');
			if (imageHeight < this.imageWrapperHeight) {
				var dif = this.imageWrapperHeight - imageHeight;
				imgContainer.css('top', (dif / 2) +'px');
			};
			imgContainer.css('left', '0px');
			if (imageWidth < this.imageWrapperWidth) {
				var dif = this.imageWrapperWidth - imageWidth;
				imgContainer.css('left', (dif / 2) +'px');
			};
		},

		_createDescription: function(image) {
			var desc = null;
			if (image.desc.length || image.title.length) {
				var title = '';
				if (image.title.length) {
			  		title = '<strong class="sg-description-title ellipsis" title="{0}">{0}</strong>'.format(image.title);
				};
				var desc = '';
				if (image.desc.length) {
			  		desc = '<span>'+ image.desc +'</span>';
				};
				desc = $('<div class="sg-image-description">'+ title + desc +'</div>');
			};
			return desc;
		},

		showImage: function(index, callback) {
			if (this.images[index] && !this.inTransition) {
				var self = this;
				var image = this.images[index];
				this.inTransition = true;
				if (!image.preloaded) {
					this.loading(true);
					this.preloadImage(index, function() {
						self.loading(false);
						self._showWhenLoaded(index, callback);
					});
				} 
				else {
					this._showWhenLoaded(index, callback);
				};
			};
		},

		_showWhenLoaded: function(index, callback) {
			if (!this.images[index]) return;
			
			var self = this;
			var image = this.images[index];
			var opts = self.options;

			var imgContainer = $(document.createElement('div')).addClass('sg-image');
			var img = $(new Image()).attr('src', image.image);
			if (image.link) {
				var link = $('<a href="'+ image.link +'" target="_blank"></a>');
				link.append(img);
				imgContainer.append(link);
			} 
			else {
				imgContainer.append(img);
			}
			
			this.imageWrapper.prepend(imgContainer);
			var size = this._getContainedImageSize(image.size.width, image.size.height);
			img.attr('width', size.width);
			img.attr('height', size.height);
			imgContainer.css({ width: size.width +'px', height: size.height +'px' });
			this._centerImage(imgContainer, size.width, size.height);
			
			if (opts.enableDescription) {
				var desc = this._createDescription(image, imgContainer);
				if (desc) {
					this.imageWrapper.append(desc);
					var width = this.imageWrapper.width() - parseInt(desc.css('padding-left'), 10) - parseInt(desc.css('padding-right'), 10);
					desc.css('width', width + 'px');
					desc.css('bottom', '-'+ desc[0].offsetHeight +'px');
				};
			}
			
			this.highlightThumb(this.nav.find('.sg-thumb'+ index));
			
			var direction = 'right';
			if (this.currentIndex < index) {
				direction = 'left';
			};
			
			if (this.currentImage) {
			    var animationSpeed = opts.animationSpeed;
				var easing = defaultEasing;
				var animation = (animations[opts.animation] || animations['none']).call(this, imgContainer, direction, desc);
				if (typeof animation.speed != 'undefined') {
					animationSpeed = animation.speed;
				};
				if (typeof animation.easing != 'undefined') {
					easing = animation.easing;
				};
				
				var oldImage = this.currentImage;
				var oldDescription = this.currentDescription;
				oldImage.animate(animation.oldImage, animationSpeed, easing, function() {
				    var oldZoomImage = oldImage.find("img").data("elevateZoom");
				    if (oldZoomImage) {
				        oldZoomImage.reset();
				    }
				    oldImage.remove();
				    if (oldDescription) oldDescription.remove();
				});
				
				imgContainer.animate(animation.newImage, animationSpeed, easing, function() {
					self.currentIndex = index;
					self.currentImage = imgContainer;
					self.currentDescription = desc;
					self.inTransition = false;
					self.fireCallback(callback);
				});
			  
			} 
			else {
				this.currentIndex = index;
				this.currentImage = imgContainer;
				this.currentDescription = desc;
				this.inTransition = false;
				this.fireCallback(callback);
			};

		    // enable zoom
			if ($.isPlainObject(opts.zoom) && opts.zoom.enabled === true && image.zoom) {
			    img.attr("data-zoom-image", image.zoom);

			    var zoomOpts = $.extend({}, defaultZoomOpts, opts.zoom);

			    if (!zoomOpts.zoomWindowHeight)
			        zoomOpts.zoomWindowHeight = self.el.height() - 2;

			    if (zoomOpts.lensShape === "round" && zoomOpts.zoomType !== "lens")
			        zoomOpts.lensShape = undefined;

			    img.elevateZoom(zoomOpts);
			}
		},

		nextIndex: function() {
			if (this.currentIndex == (this.images.length - 1)) {
				if(!this.options.cycle) {
					return false;
				};
				var next = 0;
			} 
			else {
				var next = this.currentIndex + 1;
			};
			return next;
		},

		nextImage: function(callback) {
			var next = this.nextIndex();
			if (next === false) return false;
			this.preloadImage(next + 1);
			this.showImage(next, callback);
			return true;
		},

		prevIndex: function() {
			if (this.currentIndex == 0) {
				if (!this.options.cycle) {
					return false;
				};
				var prev = this.images.length - 1;
			} 
			else {
				var prev = this.currentIndex - 1;
			};
			return prev;
		},

		prevImage: function(callback) {
		  var prev = this.prevIndex();
		  if (prev === false) return false;
		  this.preloadImage(prev - 1);
		  this.showImage(prev, callback);
		  return true;
		},

		preloadAll: function() {
			var self = this;
			var i = 0;
			function preloadNext() {
				if(i < self.images.length) {
				  i++;
				  self.preloadImage(i, preloadNext);
				};
			};
			self.preloadImage(i, preloadNext);
		},

		preloadImage: function(index, callback) {
			if (this.images[index]) {
				var image = this.images[index];

				if (!this.images[index].preloaded) {
					var img = $(new Image());
					img.attr('src', image.image);
					
					if (!this.isImageLoaded(img[0])) {
					  
						this.preloads.append(img);
						var self = this;
						img.load(function() {
						        image.preloaded = true;
						        //image.size = { width: this.width, height: this.height };
						        image.size = { width: $(this).width(), height: $(this).height() };
								self.fireCallback(callback);
						 	})
						 	.error(function() {
						    	image.error = true;
						    	image.preloaded = false;
						    	image.size = false;
							});
					} 
					else {
						image.preloaded = true;
						image.size = { width: img[0].width, height: img[0].height };
						this.fireCallback(callback);
					};
				  
				} 
				else {
					this.fireCallback(callback);
				}; // !this.images[index].preloaded
				};
		},

		isImageLoaded: function(img) {
			if (typeof img.complete != 'undefined' && !img.complete) {
				return false;
			};
			if (typeof img.naturalWidth != 'undefined' && img.naturalWidth == 0) {
				return false;
			};
			return true;
		},

		highlightThumb: function(thumb) {
			this.thumbsWrapper.find('.sg-active').removeClass('sg-active');
			thumb.addClass('sg-active');
			if (!this.noScrollers) {
				var left = thumb[0].parentNode.offsetLeft;
				left -= (this.navDisplayWidth / 2) - (thumb[0].offsetWidth / 2);
				this.thumbsWrapper.animate({ scrollLeft: left +'px' });
				this._toggleScrollButtons(left);
			}
		},

		fireCallback: function(fn) {
			if($.isFunction(fn)) {
				return fn.call(this);
			};
		}		
		
	} // SmartGallery.prototype
	
	
	// the global, default plugin options
	_.provide('$.' + pluginName);
	$[pluginName].defaults = {
        // whether the gallery should be update on windows/viewport resize
	    responsive: true,
        // the max width and/or height of thumbnails
        thumbSize: 50,
		// 0-based index of image to start with
		startIndex: 0,
        // animation type: slide, fade, none
		animation: "slide",
		// speed of animation in ms.
		animationSpeed: 400,
		// width of outermost container in px.
        width: null,
        // height of outermost container in px.
        height: null,
        // display the top toolbar
        displayImage: true,
        // ...
        scrollJump: 0,
        // cycle thru images
        cycle: true,
        // show the bottom description panel
        enableDescription: true,
        // ...
		enableKeyboardMove: true,
        // ...
		ensureLink: true,
		// zoom options
		zoom: {
			enabled: true,
            zoomType: "window",
			/* {...} 'elevatedZoom' options are passed through */
		},	
		// full size image box options
		box: {
			enabled: true
			/* {...} bootstrap image gallery options are passed through */
		},
		callbacks: {
			imageClick: null,
			thumbClick: null
		}	
	}
	
	$.fn[pluginName] = function( options ) {

		return this.each(function() {
		    if (!$.data(this, 'plugin_' + pluginName)) {
		        options = $.extend( true, {}, $[pluginName].defaults, options );
		        
		        $.data(this, 'plugin_' + pluginName, new SmartGallery( this, options ));
		    }
		});
	}

})(jQuery, window, document);
