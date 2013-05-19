
;
(function ($) {

    $.sequence = $.sequence || {};

    $.extend($.sequence, {

        applyTheme: function (sequence /* instance */, parallaxBgSelector) {

            var nav = $('<nav class="nav" />');
            var dots = nav.children();

            sequence.afterLoaded = function () {
                if (sequence.numberOfFrames > 1) {
                    for (var i = 0; i < sequence.numberOfFrames; ++i) {
                        nav.append('<span />');
                    }
                    nav.prependTo(sequence.container);

                    dots = nav.children();

                    dots.eq(0).addClass("active");
                    nav.find(':nth-child(' + sequence.settings.startingFrameID + ')').addClass("active");

                    // nav event handler
                    nav.on("click", "span:not(.active)", function () {
                        if (!sequence.active) {
                            $(this).removeClass("active").addClass("active");
                            sequence.nextFrameID = $(this).index() + 1;
                            sequence.goTo(sequence.nextFrameID);
                        }
                    });
                }
            }

            sequence.beforeNextFrameAnimatesIn = function () {
                nav.find(':not(:nth-child(' + sequence.nextFrameID + '))').removeClass("active");
                nav.find(':nth-child(' + sequence.nextFrameID + ')').addClass("active");
            }


            var bgincrement = 50;
            var bgpositer = 0;
            var parallaxBgContainer = (!_.isString(parallaxBgSelector) ? sequence.container : sequence.container.closest(parallaxBgSelector));
            parallaxBgContainer.css('background-position', '0 50%');
            sequence.beforeCurrentFrameAnimatesOut = function () {
                sequence.direction === 1 ? --bgpositer : ++bgpositer
                if (Modernizr.csstransitions) {
                    parallaxBgContainer.css('background-position-x', bgpositer * bgincrement + '%');
                }
                else {
                    // IE9 & others
                    parallaxBgContainer.stop().animate(
                        { 'background-position': '{0}={1}% +=0%'.format(sequence.direction === 1 ? "+" : "-", bgincrement) },
                        { duration: sequence.settings.transitionThreshold, easing: "easeInOutQuad" });
                }
            };

        }

    });

})(jQuery);