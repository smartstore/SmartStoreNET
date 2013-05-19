/*
* jquery.requestAnimationFrame
* https://github.com/gnarf37/jquery-requestAnimationFrame
* Requires jQuery 1.8+
*
* Copyright (c) 2012 Corey Frang
* Licensed under the MIT license.
*/

(function ($) {

    // FireFox apparently doesn't like using this from a variable...
    window.requestAnimationFrame = window.requestAnimationFrame ||
	window.webkitRequestAnimationFrame ||
	window.mozRequestAnimationFrame ||
	window.oRequestAnimationFrame ||
	window.msRequestAnimationFrame;

    var animating;

    function raf() {
        if (animating) {
            window.requestAnimationFrame(raf);
            jQuery.fx.tick();
        }
    }

    if (window.requestAnimationFrame) {

        jQuery.fx.timer = function (timer) {
            if (timer() && jQuery.timers.push(timer) && !animating) {
                animating = true;
                raf();
            }
        };

        jQuery.fx.stop = function () {
            animating = false;
        };

    }

} (jQuery));