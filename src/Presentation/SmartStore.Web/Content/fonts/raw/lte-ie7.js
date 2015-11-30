/* Use this script if you need to support IE 7 and IE 6. */

window.onload = function() {
	function addIcon(el, entity) {
		var html = el.innerHTML;
		el.innerHTML = '<span style="font-family: \'Fontastic\'">' + entity + '</span>' + html;
	}
	var icons = {
			'sm-icon-database' : '&#x21;',
			'sm-icon-user' : '&#x22;',
			'sm-icon-cog' : '&#x23;',
			'sm-icon-stats-up' : '&#x24;',
			'sm-icon-gift' : '&#x25;',
			'sm-icon-cube' : '&#x26;',
			'sm-icon-help' : '&#x27;',
			'sm-icon-loop' : '&#x28;',
			'sm-icon-tag' : '&#x29;',
			'sm-icon-gift-2' : '&#x2b;',
			'sm-icon-home' : '&#x2d;',
			'sm-icon-earth' : '&#x2e;',
			'sm-icon-loop-alt2' : '&#x33;',
			'sm-icon-layers' : '&#x34;',
			'sm-icon-tag-stroke' : '&#x36;',
			'sm-icon-archive' : '&#x3c;',
			'sm-icon-cog-2' : '&#x3d;',
			'sm-icon-refresh' : '&#x3e;',
			'sm-icon-tag-2' : '&#x3f;',
			'sm-icon-tag-3' : '&#x40;',
			'sm-icon-chart' : '&#x43;',
			'sm-icon-cube-2' : '&#x44;',
			'sm-icon-megaphone' : '&#x45;',
			'sm-icon-box' : '&#x46;',
			'sm-icon-layout' : '&#x47;',
			'sm-icon-layout-2' : '&#x48;',
			'sm-icon-layout-3' : '&#x49;',
			'sm-icon-layout-4' : '&#x4a;',
			'sm-icon-stats' : '&#x4b;',
			'sm-icon-discout' : '&#x4c;',
			'sm-icon-retweet' : '&#x4d;',
			'sm-icon-tags' : '&#x4e;',
			'sm-icon-users' : '&#x4f;',
			'sm-icon-user-2' : '&#x50;',
			'sm-icon-price' : '&#x51;',
			'sm-icon-retweet-2' : '&#x52;',
			'sm-icon-download' : '&#x53;',
			'sm-icon-upload' : '&#x54;',
			'sm-icon-home-2' : '&#x55;',
			'sm-icon-dots-three' : '&#x56;',
			'sm-icon-users-2' : '&#x57;',
			'sm-icon-contact' : '&#x2a;',
			'sm-icon-camera' : '&#x2c;',
			'sm-icon-cart' : '&#x2f;',
			'sm-icon-bag' : '&#x30;',
			'sm-icon-shopping' : '&#x31;',
			'sm-icon-cart-2' : '&#x32;',
			'sm-icon-cart-3' : '&#x35;',
			'sm-icon-cart-4' : '&#x37;',
			'sm-icon-basket' : '&#x38;'
		},
		els = document.getElementsByTagName('*'),
		i, attr, html, c, el;
	for (i = 0; i < els.length; i += 1) {
		el = els[i];
		attr = el.getAttribute('data-icon');
		if (attr) {
			addIcon(el, attr);
		}
		c = el.className;
		c = c.match(/sm-icon-[^\s'"]+/);
		if (c && icons[c[0]]) {
			addIcon(el, icons[c[0]]);
		}
	}
};