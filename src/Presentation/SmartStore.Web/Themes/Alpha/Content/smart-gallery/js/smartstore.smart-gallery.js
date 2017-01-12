/*
 *  Project: smartstore.smartgallery
 *  Version: 2.0
 *  Author: Murat Cakir
 */

;(function ($, window, document, undefined) {
    
	var pluginName = 'smartGallery';
	var isTouch = Modernizr.touchevents;

	var defaultZoomOpts = {
		// Prefix for generated element class names (e.g. `my-ns` will
		// result in classes such as `my-ns-pane`. Default `drift-`
		// prefixed classes will always be added as well.
		namespace: null,
		// Whether the ZoomPane should show whitespace when near the edges.
		showWhitespaceAtEdges: false,
		// Whether the inline ZoomPane should stay inside
		// the bounds of its image.
		containInline: true,
		// How much to offset the ZoomPane from the
		// interaction point when inline.
		inlineOffsetX: 0,
		inlineOffsetY: 0,
		// A DOM element to append the inline ZoomPane to.
		inlineContainer: document.body,
		// Which trigger attribute to pull the ZoomPane image source from.
		sourceAttribute: 'data-zoom',
		// How much to magnify the trigger by in the ZoomPane.
		// (e.g., `zoomFactor: 3` will result in a 900 px wide ZoomPane image
		// if the trigger is displayed at 300 px wide)
		zoomFactor: 3,
		// A DOM element to append the non-inline ZoomPane to.
		// Required if `inlinePane !== true`.
		paneContainer: document.body,
		// When to switch to an inline ZoomPane. This can be a boolean or
		// an integer. If `true`, the ZoomPane will always be inline,
		// if `false`, it will switch to inline when `windowWidth <= inlinePane`
		inlinePane: 768,
		// If `true`, touch events will trigger the zoom, like mouse events.
		handleTouch: true,
		// If present (and a function), this will be called
		// whenever the ZoomPane is shown.
		onShow: null,
		// If present (and a function), this will be called
		// whenever the ZoomPane is hidden.
		onHide: null,
		// Add base styles to the page. See the "Theming"
		// section of README.md for more information.
		injectBaseStyles: true,
		// An optional number that determines how long to wait before
		// showing the ZoomPane because of a `mouseenter` event.
		hoverDelay: 150,
		// An optional number that determines how long to wait before
		// showing the ZoomPane because of a `touchstart` event.
		// It's unlikely that you would want to use this option, since
		// "tap and hold" is much more intentional than a hover event.
		touchDelay: 0,
		// If true, a bounding box will show the area currently being previewed
		// during mouse hover
		hoverBoundingBox: true,
		// If true, a bounding box will show the area currently being previewed
		// during touch events
		touchBoundingBox: true
	};
	
	function SmartGallery( element, options ) {
		var self = this;

		this.element = element;
		var el = this.el = $(element);

		var meta = $.metadata ? $.metadata.get(element) : {};
		var opts = this.options = $.extend(true, {}, options, meta || {});

		this.init = function () {
			var self = this;

			var startAt = parseInt(opts.startIndex, 10);
			if (window.location.hash && window.location.hash.indexOf('#sg-image') === 0) {
				startAt = window.location.hash.replace(/[^0-9]+/g, '');
				if (_.isNumber(startAt)) {
					opts.startIndex = startAt;
				}
			}

			this.zoomWindowContainer = $('.zoom-window-container');

			this.initNav();
			this.initGallery();

			if ($.isPlainObject(opts.zoom) && opts.zoom.enabled === true) {
				this.initZoom();
			}			

			if ($.isPlainObject(opts.box) && opts.box.enabled) {
				this.initBox();
				this.initModal();
			}
		};
		
		this.init2 = function(isRefresh) {
			var self = this;
			this.setupElements(); // DO
			
			this.imageWrapperWidth = this.imageWrapper.width();
			this.imageWrapperHeight = this.imageWrapperWidth;

			//this.imageWrapper.css({ height: this.imageWrapperHeight + 'px' });
			
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
			}
		
			//this.findImages();
			
			var startAt = parseInt(opts.startIndex, 10);
			if (window.location.hash && window.location.hash.indexOf('#sg-image') === 0) {
				startAt = window.location.hash.replace(/[^0-9]+/g, '');
				if(!_.isNumber(startAt)) {
					startAt = opts.startIndex;
				}
			}
			
			if (this.images[startAt]) {
				this.loading(true);
			}
			
			this.showImage(startAt);

			// handle pan/swipe
			var mc = new Hammer(this.imageWrapper[0]);
			mc.on("swipeleft swiperight panleft panright panend", function (e) {
				if (e.type === "swipeleft") {
					self.nextImage();
				}
				else if (e.type === "swiperight") {
					self.prevImage();
				}
			});

			if (opts.responsive && !isRefresh) {
				EventBroker.subscribe("page.resized", function (msg, viewport) {
				    //self.reset();
				    //self.inTransition = false;
					//self.init(true);
			    });
			}
		};
		
		this.initialized = false;
		this.init();
		this.initialized = true;	
	}
	
	SmartGallery.prototype = {
		gallery: null,
		nav: null,
		navList: null,
		navTrack: null,
		navPrevArrow: null,
		navNextArrow: null,
		navItemsCount: 0,
		zoomWindowContainer: null,
		modal: null,

		imageWrapper: null,
		loader: null,
		preloads: null,
		thumbsWrapper: null,
		box: null, // (blueImp) image gallery element

		imageWrapperWidth: 0,
		imageWrapperHeight: 0,
		currentIndex: 0,
		currentImage: null,
		navDisplayWidth: 0,
		navListWidth: 0,
		noScrollers: false,
		images: null,
		inTransition: false,
		inWrapper: false,

		scrollForward: null,
		scrollBack: null,

		origHtml: null,

		initGallery: function () {
			var self = this;
			var gal = self.el.find(".gal");
			if (gal.length === 0) {
				return;
			}

			self.gallery = gal;

			var options = {
				infinite: false,
				lazyLoad: "ondemand",
				dots: true,
				arrows: false,
				//prevArrow: '<button type="button" class="btn btn-secondary btn-flat btn-circle slick-prev"><i class="fa fa-angle-left"></i></button>',
				//nextArrow: '<button type="button" class="btn btn-secondary btn-flat btn-circle slick-next"><i class="fa fa-angle-right"></i></button>',
				cssEase: 'ease-in-out',
				speed: isTouch ? 250 : 0,
				useCSS: true,
				useTransform: true,
				waitForAnimate: true,
				slidesToShow: 1,
				slidesToScroll: 1,
				initialSlide: self.options.startIndex
			};

			self.currentIndex = self.options.startIndex;

			gal.slick(options);

			gal.height(gal.width());
			EventBroker.subscribe("page.resized", function (msg, viewport) {
				gal.height(gal.width());
				self.initNav();
			});

			gal.on('beforeChange', function (e, slick, curIdx, nextIdx) {
				if (!self.nav.data('glimpse')) {
					// Sync with thumb nav
					self._selectNavItem(nextIdx, false);
				}
			});

			gal.on('afterChange', function (e, slick, currentSlide) {
				self.currentIndex = currentSlide;
			});

			if (!isTouch) {
				gal
					.on('mouseenter.gal', function (e) { gal.slick("slickSetOption", "speed", 250); })
					.on('mouseleave.gal', function (e) { gal.slick("slickSetOption", "speed", 0); });
			}
		},

		initNav: function () {
			var self = this;
			self.nav = (nav = (self.nav || self.el.find('.gal-nav')));
			if (nav.length === 0)
				return;

			var isInitialized = nav.hasClass('gal-initialized');

			nav.off('.gal');

			var list = self.navList = nav.find('.gal-list').first(),
				track = self.navTrack = list.find('.gal-track').first(),
				items = list.find('.gal-item'),
				itemHeight = items.first().outerHeight(true);

			self.navItemsCount = items.length;

			items.each(function (i, el) {
				var $el = $(this);
				$el.attr('data-gal-index', i);
			});

			if (items.length > self.options.thumbsToShow) {
				if (!isInitialized) {
					self.navPrevArrow = $('<button type="button" class="btn btn-secondary btn-flat btn-circle gal-arrow gal-prev gal-disabled"><i class="fa fa-angle-up" style="vertical-align: top"></i></button>').prependTo(nav);
					self.navNextArrow = $('<button type="button" class="btn btn-secondary btn-flat btn-circle gal-arrow gal-next gal-disabled"><i class="fa fa-angle-down"></i></button>').appendTo(nav);
				}

				list.height(itemHeight * self.options.thumbsToShow);

				nav.on('click.gal', '.gal-arrow', function (e) {
					e.preventDefault();
					var btn = $(this);

					if (btn.hasClass('gal-disabled')) {
						return;
					}
					else if (btn.hasClass('gal-prev')) {
						self._slideToPrevNavPage();
					}
					else if (btn.hasClass('gal-next')) {
						self._slideToNextNavPage();
					}

					return false;
				});
			}

			self._selectNavItem(self.options.startIndex, isInitialized);

			nav.on('mouseenter.gal click.gal', '.gal-item', function (e) {
				e.preventDefault();

				if (e.type === "mouseenter") {
					nav.data("glimpse", true);
				}

				var toIdx = $(this).data('gal-index');
				self.gallery.slick("slickGoTo", toIdx);

				if (e.type === "click") {
					// sync with gallery
					nav.data("glimpse", false);
					self._selectNavItem(toIdx, true);
				}

				return false;
			})
			.on('mouseleave.gal', function (e) {
				// Restore actual selected image
				var actualIdx = nav.find('.gal-current').data('gal-index');
				self.gallery.slick("slickGoTo", actualIdx);
				nav.data("glimpse", false);
			});

			nav.addClass("gal-initialized");
		},

		_selectNavItem: function (idx, sync) {
			var self = this;
			var curItem = self.nav.find('.gal-current');
			var curIdx = curItem.data('gal-index');
			if (curIdx === idx)
				return;

			curItem.removeClass('gal-current');
			curItem = self.nav.find('[data-gal-index=' + idx + ']');
			curItem.addClass('gal-current');

			var page = Math.floor(idx / self.options.thumbsToShow);
			self._slideToNavPage(page);

			if (sync) {
				self.gallery.slick('slickGoTo', idx);
			}
		},

		_slideToPrevNavPage: function () {
			var curPage = this.nav.data('current-page');
			this._slideToNavPage(curPage - 1);
		},

		_slideToNextNavPage: function () {
			var curPage = this.nav.data('current-page');
			this._slideToNavPage(curPage + 1);
		},

		_slideToNavPage: function(page) {
			if (this.nav.data('current-page') !== page) {
				this.nav.data('current-page', page);

				var hasArrows = this.navPrevArrow && this.navNextArrow;

				if (page === 0 && hasArrows) {
					this.navPrevArrow.addClass('gal-disabled');
					this.navNextArrow.removeClass('gal-disabled');
				}
				else if (page > 0 && hasArrows) {
					this.navPrevArrow.removeClass('gal-disabled');
					var isLastPage = page >= Math.floor(this.navItemsCount / this.options.thumbsToShow);
					this.navNextArrow.toggleClass('gal-disabled', isLastPage);
				}

				var navListHeight = this.navList.height();
				var maxOffsetY = (this.navTrack.height() - navListHeight) * -1;
				var offsetY = navListHeight * page * -1;
				this.navTrack.css(Modernizr.prefixedCSS('transform'), 'translate3d(0, ' + Math.max(offsetY, maxOffsetY) + 'px, 0)');
			}
		},

		initZoom: function () {
			if (isTouch)
				return; // no zoom on touch devices

			var self = this;

			self.gallery.on('beforeChange', function (e, slick, curIdx, nextIdx) {
				// destroy zoom
				if (self.nav.data('glimpse'))
					return;
				var img = self.gallery.find('.gal-item').eq(curIdx).find('img');
				var drift = img.data("drift");
				if (drift) {
					drift.disable();
					img.data("drift", null);
				}
			});

			self.gallery.on('afterChange', function (e, slick, idx) {
				// apply zoom
				if (self.nav.data('glimpse'))
					return;
				applyZoom(self.gallery.find('.gal-item').eq(idx));
			});

			function applyZoom(slide) {
				var a = slide.find('> a');
				var img = slide.find('img');

				if (img.data("drift") || self.zoomWindowContainer.length === 0)
					return;

				var zoomOpts = $.extend({}, defaultZoomOpts, self.options.zoom, {
					paneContainer: self.zoomWindowContainer[0],
					onShow: function () {
						_.delay(function () {
							self.zoomWindowContainer.find('.drift-zoom-pane').height(a.outerHeight());
							console.log(self.zoomWindowContainer.find('.drift-zoom-pane').height());
						}, 10);
						
						// Fix Drift issue: boundingBox parent must be body, NOT image's parent link/viewport
						drift.trigger.boundingBox.settings.containerEl = document.body;
					}
				});

				var drift = new Drift(img[0], zoomOpts);
				img.data('drift', drift);
			}

			// Apply on first init
			var curIndex = self.gallery.slick('slickCurrentSlide');
			applyZoom(self.gallery.find('.gal-item').eq(curIndex));
		},

		initModal: function() {
			var self = this;

			var getModal = function (id) {
				var modal = $('#' + id);
				if (modal.length > 0) {
					return modal;
				}

				var html =
					'<div id="' + id + '" class="modal modal-flex gal-modal fade">'
					+ '<div class="modal-dialog" role="document">'
					+ '<div class="modal-content">'
					+ '<div class="modal-body p-a-0">'
					+ '</div>'
					+ '</div>'
					+ '</div>'
					+ '</div>';

				modal = $(html);
				modal.appendTo('body');

				var modalBody = modal.find('.modal-body');
				var links = $('[data-gallery="' + (id === 'image-gallery-default' ? 'default' : id) + '"]');
				console.log(links.length);
				var slickHtml = '<div class="gal">';
				links.each(function (i, el) {
					var a = $(el);
					var img = a.find('>img');
					slickHtml += '<div class="gal-item">';
					slickHtml += '<img class="x-gal-item-content" src="' + a.attr('href') + '" alt="' + img.attr('alt') + '" />';
					slickHtml += '</div>';
				});
				slickHtml += '</div>';

				var slick = $(slickHtml).appendTo(modalBody);
				
				slick.slick({
					infinite: false,
					lazyLoad: "ondemand",
					dots: false,
					arrows: false,
					//prevArrow: '<button type="button" class="btn btn-secondary btn-flat btn-circle slick-prev"><i class="fa fa-angle-left"></i></button>',
					//nextArrow: '<button type="button" class="btn btn-secondary btn-flat btn-circle slick-next"><i class="fa fa-angle-right"></i></button>',
					cssEase: 'ease-in-out',
					speed: 250,
					useCSS: true,
					useTransform: true,
					waitForAnimate: true,
					slidesToShow: 1,
					slidesToScroll: 1,
					initialSlide: self.options.startIndex
				});

				self.modal = modal;

				//var prevY = null;

				//var onMouseMove = function (e) {
				//	if (!_.isNumber(prevY)) {
				//		prevY = e.pageY;
				//		return;
				//	}

				//	if (e.pageY - prevY > 25) {
				//		// moved down by 25px
				//		modal.find('>.indicator').removeClass('out').data("explicit-move", true);
				//	}
				//	else if (prevY - e.pageY > 25) {
				//		// moved up by 25px
				//		modal.find('>.indicator').addClass('out');
				//	}

				//	prevY = e.pageY;
				//};

				//// trigger mousemove all 100ms
				//var throttledMouseMove = _.throttle(onMouseMove, 100, { leading: false, trailing: false });
				//modal.find('> .slides').on('mousemove', throttledMouseMove);

				return modal;
			};

			if (self.gallery) {
				// Global click handler to open links with data-gallery attribute
				// in the Gallery lightbox:
				self.gallery.on('click', '.gal-item > a', function (e) {
					e.preventDefault();

					var id = $(this).data('gallery') || 'default',
						modal = getModal(id === 'default' ? 'image-gallery-default' : id),
						// Select all links with the same data-gallery attribute:
						links = $('[data-gallery="' + id + '"]').not($(this));

					modal.modal('show');

					return false;
				});
			}
		},

		initZoomOld: function () {
			if (isTouch)
				return; // no zoom on touch devices
			
			var self = this;

			self.gallery.on('beforeChange', function (e, slick, curIdx, nextIdx) {
				// destroy zoom
				if (self.nav.data('glimpse'))
					return;
				var img = self.gallery.find('.gal-item').eq(curIdx).find('img');
				var zoomObj = img.data("elevateZoom");
				if (zoomObj) {
					zoomObj.reset();
					img.data("elevateZoom", null);
				}
			});

			self.gallery.on('afterChange', function (e, slick, idx) {
				// apply zoom
				if (self.nav.data('glimpse'))
					return;
				applyZoom(self.gallery.find('.gal-item').eq(idx));
			});

			function applyZoom(slide) {
				var href = slide.find('> a').attr('href');
				var img = slide.find('img');

				if (img.data("elevateZoom"))
					return;

				img.attr("data-zoom-image", href);
				var zoomOpts = $.extend({}, defaultZoomOpts, self.options.zoom);

				if (!zoomOpts.zoomWindowWidth)
					zoomOpts.zoomWindowWidth = $('#content').width() - self.el.width() - (zoomOpts.zoomWindowOffetX || 10);

				if (!zoomOpts.zoomWindowHeight)
					zoomOpts.zoomWindowHeight = self.el.height() - 2;

				if (!zoomOpts.zoomWindowOffetY)
					zoomOpts.zoomWindowOffetY = (img.offset().top - self.el.offset().top - 300) * -1;

				console.log(zoomOpts.zoomWindowOffetY);

				if (zoomOpts.lensShape === "round" && zoomOpts.zoomType !== "lens")
					zoomOpts.lensShape = undefined;

				img.elevateZoom(zoomOpts);
			}

			// Apply on first init
			var curIndex = self.gallery.slick('slickCurrentSlide');
			applyZoom(self.gallery.find('.gal-item').eq(curIndex));
		},

		setupElements: function () {
			var el = this.el;

			this.origHtml = this.el.outerHtml();
			this.imageWrapper = el.find('.sg-image-wrapper').empty();
			this.nav = el.find('.sg-nav').css("opacity", "0");
			this.thumbsWrapper = this.nav.find('.sg-thumbs');
			this.preloads = $('<div class="sg-preloads"></div>');
			this.loader = $('<div class="spinner-container sg-loader"></div>').append(createCircularSpinner(24, false, null, true));
			this.imageWrapper.append(this.loader);
			$(document.body).append(this.preloads);
		},

		reset: function () {
			var self = this;

			self.imageWrapperWidth = 0;
			self.imageWrapperHeight = 0;
			self.navDisplayWidth = 0;
			self.navListWidth = 0;
			self.noScrollers = false;
			self.nav.removeClass("has-buttons");
			self.thumbsWrapper.find('> ul > li > a').off('click mouseenter');
			self.thumbsWrapper.css('width', 'initial');

			if (self.preloads) {
				self.preloads.remove();
			}
			if (self.loader) {
				self.loader.remove();
			}
			var oldZoomImage = self.imageWrapper.find('.sg-image img').data("elevateZoom");
			if (oldZoomImage) {
				oldZoomImage.reset();
			}

			self.imageWrapper.find('.sg-image').remove();

			$('.smartgallery-overlay').remove();
			if (self.box) {
				self.box.remove();
			}
			self.box = null;
		},

		loading: function (value) {
			if (value) {
				this.loader.addClass('active');
			}
			else {
				this.loader.removeClass('active');
			}
		},

		addAnimation: function (name, fn) {
			if ($.isFunction(fn)) {
				animations[name] = fn;
			}
		},

		findImages: function () {
			var self = this;
			this.images = [];
			var thumbWrapperWidth = 0;
			var thumbsLoaded = 0;
			var thumbs = this.thumbsWrapper.find('a');
			var thumbCount = thumbs.length;

			thumbs.each(function (i) {
				var link = $(this);
				var imageSrc = link.data('medium-image');
				var thumb = link.find('img');

				thumbWrapperWidth += this.parentNode.offsetWidth; // parent LI of A

				// Check if the thumb has already been loaded
				if (!self.isImageLoaded(thumb[0])) {
					thumb.load(function () {
						thumbsLoaded++;
					});
				}
				else {
					thumbsLoaded++;
				}


				link.addClass('sg-thumb' + i);

				link.on({
					'click': function () {
						var fn = function () { };
						if (!self.options.displayImage) {
							self._showBox(i);
						}
						self.showImage(i, fn);
						return false;
					},
					'mouseenter': function () {
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
				if (s && s !== title) desc = s;

				self.images[i] = {
					thumb: thumb.attr('src'),
					image: imageSrc,
					zoom: link.attr("href"),
					error: false,
					preloaded: false,
					desc: desc,
					title: title,
					size: false,
					link: href
				};
			});

			// Wait until all thumbs are loaded, and then set the width of the UL wrapper
			var listWrapper = self.nav.find('.sg-thumbs');
			var setULWrapperWidth = function () {
				if (thumbCount === thumbsLoaded) {
					var scrollers = self.nav.find('.scroll-button');
					if (thumbWrapperWidth <= self.navDisplayWidth) {
						// List fits into container: remove ScrollButtons
						scrollers.each(function () {
							$(this).parent().remove();
						});
						// ...and prevent a ThumbJump
						self.noScrollers = true;
					}
					else {
						// List ist wider than container: we need ScrollButtons	
						if (scrollers.length > 0) {
							self.nav.addClass("has-buttons"); // der hier hat die paddings
							var parentHeight = scrollers.parent().innerHeight();
							scrollers.height(parentHeight - scrollers.verticalCushioning());
						}
					}

					thumbWrapperWidth += listWrapper.horizontalCushioning() - thumbsLoaded + 1;

					// tatsächliche Breite der Liste setzen
					listWrapper.css('width', thumbWrapperWidth + 'px');

					clearInterval(inter);
					self.navListWidth = thumbWrapperWidth;
					self.nav.css("opacity", "1");
				}
			};

			var inter = setInterval(setULWrapperWidth, 50);
		},

		initKeyEvents: function () {
			var self = this;
			$(document).keydown(function (e) {
				if (e.keyCode === 39) {
					// right arrow
					self.nextImage();
				}
				else if (e.keyCode === 37) {
					// left arrow
					self.prevImage();
				}
			});
		},

		initThumbNav: function () {
			var self = this;

			this.scrollForward = $('<a href="#" class="sb invisible sb-dir-right">></a>');
			this.scrollBack = $('<a href="#" class="sb invisible sb-dir-left"><</a>');
			this.nav.prepend(this.scrollForward.wrap('<div class="sg-scroll-forward"></div>').parent());
			this.nav.prepend(this.scrollBack.wrap('<div class="sg-scroll-back"></div>').parent());

			var hasScrolled = 0;
			var thumbsScrollInterval = false;

			return;
			this.nav.find(".sb").scrollButton({
				nearSize: "100%",
				farSize: "100%",
				showButtonAlways: true,
				autoPosition: false,
				position: "outside",
				offset: 4,
				handleCorners: false,
				smallIcons: true,
				click: function (dir) {
					// We don't want to jump the whole width, since an image
					// might be cut at the edge
					var width = self.navDisplayWidth - 80;
					if (self.options.scrollJump >>> 0) {
						width = self.options.scrollJump;
					}
					var left;
					if (dir === 'right') {
						left = self.thumbsWrapper.scrollLeft() + width;
					}
					else {
						left = self.thumbsWrapper.scrollLeft() - width;
					}
					left = Math.min(left, self.navListWidth - self.navDisplayWidth);

					self.thumbsWrapper.animate({ scrollLeft: left + 'px' }, 400, "easeOutQuad");
					self._toggleScrollButtons(left);
					return false;
				},
				enter: function (dir) {
					thumbsScrollInterval = setInterval(function () {
						hasScrolled++;
						var left = self.thumbsWrapper.scrollLeft() + 1;
						if (dir === 'left') {
							left = self.thumbsWrapper.scrollLeft() - 1;
						}
						self.thumbsWrapper.scrollLeft(left);
						self._toggleScrollButtons(left);
					}, 10);
				},
				leave: function (dir) {
					hasScrolled = 0;
					clearInterval(thumbsScrollInterval);
				}
			});
		},

		initBox: function () {
			var self = this;

			var getWidget = function (id) {
				var widget = $('#' + id);
				if (widget.length > 0) {
					return widget;
				}

				widget = $('<div id="' + id + '" class="blueimp-gallery blueimp-gallery-controls"></div>')
					.append('<div class="slides"></div>')
					.append('<a class="prev"><i class="fa fa-angle-left"></i></a>')
					.append('<a class="next"><i class="fa fa-angle-right"></i></a>')
					.append('<a class="close">' + String.fromCharCode(215) + '</a>')
					.append('<ol class="indicator"></ol>');
				widget.appendTo('body');

				self.box = widget;

				var prevY = null;

				var onMouseMove = function (e) {
					if (!_.isNumber(prevY)) {
						prevY = e.pageY;
						return;
					}

					if (e.pageY - prevY > 25) {
						// moved down by 25px
						widget.find('>.indicator').removeClass('out').data("explicit-move", true);
					}
					else if (prevY - e.pageY > 25) {
						// moved up by 25px
						widget.find('>.indicator').addClass('out');
					}

					prevY = e.pageY;
				};

				// trigger mousemove all 100ms
				var throttledMouseMove = _.throttle(onMouseMove, 100, { leading: false, trailing: false });
				widget.find('> .slides').on('mousemove', throttledMouseMove);

				return widget;
			};

			var getOverlay = function (widget) {
				var gov = $('.smartgallery-overlay');
				if (gov.length === 0) {
					gov = $('<div class="smartgallery-overlay" style="display: none"></div>').insertBefore(widget[0]);
				}
				return gov;
			};

			if (self.gallery) {
				// Global click handler to open links with data-gallery attribute
				// in the Gallery lightbox:
				self.el.on('click', '.oldgal', function (e) {
					e.preventDefault();

					var id = $(this).data('gallery') || 'default',
						widget = getWidget(id === 'default' ? 'image-gallery-default' : id),
						container = (widget.length && widget) || $(Gallery.prototype.options.container),
						callbacks = {
							onopen: function () {
								var gov = getOverlay(widget);
								gov.on('click', function (e) {
									widget.data('gallery').close();
								});
								gov.show().addClass("in");
								container.data('gallery', this).trigger('open');
							},
							onopened: function () {
								container.trigger('opened');
							},
							onslide: function () {
								container.trigger('slide', arguments);
							},
							onslideend: function () {
								container.trigger('slideend', arguments);
							},
							onslidecomplete: function () {
								container.trigger('slidecomplete', arguments);
							},
							onclose: function () {
								getOverlay(widget).removeClass("in");
								container.trigger('close');
							},
							onclosed: function () {
								getOverlay(widget).css('display', 'none');
								container.trigger('closed').removeData('gallery');
							}
						},
						options = $.extend(
							// Retrieve custom options from data-attributes
							// on the Gallery widget:
							container.data(),
							{
								container: container[0],
								index: self.currentIndex,
								event: e
							},
							callbacks,
							self.options.box || {}
						),
						// Select all links with the same data-gallery attribute:
						links = $('[data-gallery="' + id + '"]').not($(this));

					if (options.filter) {
						links = links.filter(options.filter);
					}

					return new blueimp.Gallery(links, options);
				});
			}
		},

		_showBox: function (idx) {
			idx = idx === undefined ? this.currentIndex : idx;
			this.imageWrapper.find('.sg-image > a').click();
			var fn = this.options.callbacks.imageClick;
			return $.isFunction(fn) ? fn.call(this) : false;
		},

		_toggleScrollButtons: function (scrollLeft) {
			if (this.noScrollers) return;

			var fwd = this.scrollForward, back = this.scrollBack;
			var plugin, enabled = true;
			scrollLeft = scrollLeft !== undefined ? scrollLeft : this.thumbsWrapper.scrollLeft();
			var listWidth = this.navListWidth || this.thumbsWrapper.outerWidth(true);
			if (fwd) {
				enabled = this.navDisplayWidth - (listWidth - scrollLeft) < 0;
				plugin = fwd.data("ScrollButton");
				plugin.enable(enabled);
			}
			if (back) {
				enabled = scrollLeft > 0;
				plugin = back.data("ScrollButton");
				plugin.enable(enabled);
			}
		},

		showImage: function (index, callback) {
			if (this.initialized && index === this.currentIndex)
				return;

			if (this.images[index] && !this.inTransition) {
				var self = this;
				var image = this.images[index];
				this.inTransition = true;
				if (!image.preloaded) {
					this.loading(true);
					this.preloadImage(index, function () {
						self.loading(false);
						self._showWhenLoaded(index, callback);
					});
				}
				else {
					this._showWhenLoaded(index, callback);
				};
			};
		},

		_showWhenLoaded: function (index, callback) {
			if (!this.images[index]) return;

			var self = this;
			var image = this.images[index];
			var opts = self.options;
			var thumb = this.nav.find('.sg-thumb' + index);

			var direction = 'right';
			var inDirection = 'left';
			if (this.currentIndex < index) {
				direction = 'left';
				inDirection = 'right';
			};		

			var imgContainer = $(document.createElement('div')).addClass('sg-image');
			if (this.currentImage) {
				imgContainer.addClass('out-' + inDirection);
			}
			var img = $(new Image()).attr('src', image.image);
			if (image.link) {
				var link = $('<a class="img-center-container" href="' + image.link + '" target="_blank"></a>');
				link.append(img).data('gallery', thumb.data('gallery'));
				imgContainer.append(link);
			}
			else {
				imgContainer.addClass('img-center-container').append(img);
			}

			this.imageWrapper.prepend(imgContainer);

			this.highlightThumb(thumb); 

			if (this.currentImage) {
				var oldImage = this.currentImage;
				oldImage.addClass('out-' + direction).one($.support.transitionEnd, function (e) {
					var oldZoomImage = oldImage.find("img").data("elevateZoom");
					if (oldZoomImage) {
						oldZoomImage.reset();
					}
					oldImage.remove();
				});

				imgContainer.removeClass('out-' + inDirection).one($.support.transitionEnd, function (e) {
					self.currentIndex = index;
					self.currentImage = imgContainer;
					self.inTransition = false;
					self.fireCallback(callback);
				});
			}
			else {
				this.currentIndex = index;
				this.currentImage = imgContainer;
				this.inTransition = false;
				this.fireCallback(callback);
			};

			// enable zoom
			if ($.isPlainObject(opts.zoom) && opts.zoom.enabled === true && image.zoom) {
				if (img.data("elevateZoom") === undefined) {
					img.attr("data-zoom-image", image.zoom);
					var zoomOpts = $.extend({}, defaultZoomOpts, opts.zoom);

					if (!zoomOpts.zoomWindowHeight)
						zoomOpts.zoomWindowHeight = self.el.height() - 2;

					if (zoomOpts.lensShape === "round" && zoomOpts.zoomType !== "lens")
						zoomOpts.lensShape = undefined;

					img.elevateZoom(zoomOpts);
				}
			}
		},

		nextIndex: function () {
			var next = 0;
			if (this.currentIndex === (this.images.length - 1)) {
				if (!this.options.cycle) {
					return false;
				};
			}
			else {
				next = this.currentIndex + 1;
			};

			return next;
		},

		nextImage: function (callback) {
			var next = this.nextIndex();
			if (next === false) return false;
			this.preloadImage(next + 1);
			this.showImage(next, callback);
			return true;
		},

		prevIndex: function () {
			var prev = 0;
			if (this.currentIndex === 0) {
				if (!this.options.cycle) {
					return false;
				};
				prev = this.images.length - 1;
			}
			else {
				prev = this.currentIndex - 1;
			};

			return prev;
		},

		prevImage: function (callback) {
			var prev = this.prevIndex();
			if (prev === false) return false;
			this.preloadImage(prev - 1);
			this.showImage(prev, callback);
			return true;
		},

		preloadAll: function () {
			var self = this;
			var i = 0;
			function preloadNext() {
				if (i < self.images.length) {
					i++;
					self.preloadImage(i, preloadNext);
				};
			};
			self.preloadImage(i, preloadNext);
		},

		preloadImage: function (index, callback) {
			if (this.images[index]) {
				var image = this.images[index];

				if (!this.images[index].preloaded) {
					var img = $(new Image());
					img.attr('src', image.image);

					if (!this.isImageLoaded(img[0])) {

						this.preloads.append(img);
						var self = this;
						img.load(function () {
							image.preloaded = true;
							//image.size = { width: this.width, height: this.height };
							image.size = { width: $(this).width(), height: $(this).height() };
							self.fireCallback(callback);
						})
						 	.on("error", function (e) {
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

		isImageLoaded: function (img) {
			if (typeof img.complete !== 'undefined' && !img.complete) {
				return false;
			};
			if (typeof img.naturalWidth !== 'undefined' && img.naturalWidth === 0) {
				return false;
			};
			return true;
		},

		highlightThumb: function (thumb) {
			if (thumb.hasClass('sg-active'))
				return;

			this.thumbsWrapper.find('.sg-active').removeClass('sg-active');
			thumb.addClass('sg-active');
			if (!this.noScrollers) {
				var left = thumb[0].parentNode.offsetLeft + toInt(this.thumbsWrapper.css('margin-left'));
				left -= (this.navDisplayWidth / 2) - (thumb[0].offsetWidth / 2);
				left = Math.min(left, this.navListWidth - this.navDisplayWidth);
				this.thumbsWrapper.animate({ scrollLeft: left + 'px' });
				this._toggleScrollButtons(left);
			}
		},

		fireCallback: function (fn) {
			if ($.isFunction(fn)) {
				return fn.call(this);
			};
		}

	}; // SmartGallery.prototype
	
	
	// the global, default plugin options
	_.provide('$.' + pluginName);
	$[pluginName].defaults = {
		thumbsToShow: 6,
		// whether the gallery should be updated on windows/viewport resize
		responsive: true,
		// 0-based index of image to start with
		startIndex: 0,
		// display the top toolbar
		displayImage: true,
		// ...
		scrollJump: 0,
		// cycle thru images
		cycle: true,
		enableKeyboardMove: true,
		// ...
		ensureLink: true,
		// zoom options
		zoom: {
			enabled: true
			/* {...} 'Drift' options are passed through */
		},
		// full size image box options
		box: {
			enabled: true,
			closeOnSlideClick: false
			/* {...} blueimp image gallery options are passed through */
		},
		callbacks: {
			imageClick: null,
			thumbClick: null
		}
	};
	
	$.fn[pluginName] = function (options) {
		return this.each(function () {
			if (!$.data(this, 'plugin_' + pluginName)) {
				options = $.extend(true, {}, $[pluginName].defaults, options);

				$.data(this, 'plugin_' + pluginName, new SmartGallery(this, options));
			}
		});
	};

})(jQuery, window, document);
