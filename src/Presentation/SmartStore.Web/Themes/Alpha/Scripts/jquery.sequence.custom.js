
;
(function ($) {

    $.sequence = $.sequence || {};

    $.extend($.sequence, {

        applyTheme: function (sequence /* instance */, parallaxBgSelector) {

            var nav = $('<nav class="sequence-pagination" />');

            sequence.afterLoaded = function () {
                if (sequence.numberOfFrames > 1) {
                    for (var i = 0; i < sequence.numberOfFrames; ++i) {
                        nav.append('<span />');
                    }
                    nav.prependTo(sequence.container);
                }
            }

            sequence.settings.fadeFrameWhenSkipped = false;

            var s = sequence.settings.bgSlide || "on";

            if (s === 'on' || s === 'opposite') {
                var bgincrement = _.isNumber(sequence.settings.bgSlideIncrement) ? sequence.settings.bgSlideIncrement : 50;
                var bgpositer = 0;
                var parallaxBgContainer = (!_.isString(parallaxBgSelector) ? sequence.container : sequence.container.closest(parallaxBgSelector));

                sequence.beforeCurrentFrameAnimatesOut = function () {
                    sequence.direction === 1
                        ? (s == 'opposite' ? --bgpositer : ++bgpositer)
                        : (s == 'opposite' ? ++bgpositer : --bgpositer);
                    if (Modernizr.csstransitions) {
                        var el = parallaxBgContainer[0];
                        if (el && el.style && el.style.backgroundPositionX) {
                            parallaxBgContainer.css('background-position-x', bgpositer * bgincrement + '%');
                        }
                        else {
                            parallaxBgContainer.css('background-position', '{0} {1}'.format(bgpositer * bgincrement + '%', '50%'));
                        }
                    }
                    else {
                        // IE9 & others
                        parallaxBgContainer.stop().animate(
                            {
                                'background-position': '{0}={1}% +=0%'.format(sequence.direction === 1
                                  ? (s == 'opposite' ? "-" : "+")
                                  : (s == 'opposite' ? "+" : "-"),
                                  bgincrement)
                            },
                            { duration: sequence.settings.transitionThreshold / 2, easing: "easeInOutQuad" });
                    }
                };
            }

        }

    });

})(jQuery);