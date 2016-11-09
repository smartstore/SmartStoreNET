// jQuery.addEasing(string)
//  https://gist.github.com/1468381
// Interprets and adds easing functions to jQuery.easing
// according to the CSS spec for transition timing functions.
// 
// e.g.
// jQuery.addEasing('cubic-bezier(0.4, 0.2, 0.66, 1)');

(function(jQuery, undefined){
  
  // Timing function definitions from the CSS specification
  
  var easing = {
        'linear': [0, 0, 1, 1],
        'ease': [0.25, 0.1, 0.25, 1],
        'ease-in': [0.42, 0, 1, 1],
        'ease-out': [0, 0, 0.58, 1],
        'ease-in-out': [0.42, 0, 0.58, 1],
		'snap': [0, 1, .5, 1]
      };
  
  // Cubic bezier function purloined from
  // blogs.msdn.com/b/eternalcoding/archive/2011/12/06/css3-transitions.aspx  
  
  function makeBezier(x1, y1, x2, y2) {
    return function (t) {
      // Extract X (which is equal to time here)
      var f0 = 1 - 3 * x2 + 3 * x1,
          f1 = 3 * x2 - 6 * x1,
          f2 = 3 * x1,
          refinedT = t,
          i, refinedT2, refinedT3, x, slope;
      
      for (i = 0; i < 5; i++) {
        refinedT2 = refinedT * refinedT;
        refinedT3 = refinedT2 * refinedT;
        
        x = f0 * refinedT3 + f1 * refinedT2 + f2 * refinedT;
        slope = 1.0 / (3.0 * f0 * refinedT2 + 2.0 * f1 * refinedT + f2);
        refinedT -= (x - t) * slope;
        refinedT = Math.min(1, Math.max(0, refinedT));
      }
     
      return 3 * Math.pow(1 - refinedT, 2) * refinedT * y1 +
        3 * (1 - refinedT) * Math.pow(refinedT, 2) * y2 +
        Math.pow(refinedT, 3);
    };
  }
  
  function addEasing(str) {
    var fn = jQuery.easing[str],
        name, coords, l;
    
    // Return the cached function if it exists.
    if (fn) { return fn; }
    
    // Get the standard easing function if it's defined.
    if (easing[str]) {
      name = str;
      coords = easing[str];
      str = 'cubic-bezier(' + coords.join(', ') + ')';
    }
    
    // Else assume this is a cubic-bezier. It must be.
    else {
      coords = str.match(/\d*\.?\d+/g);
      l = coords.length; // Should be 4
      
      while (l--) {
        coords[l] = parseFloat(coords[l]);
      }
    }
    
    fn = makeBezier.apply(this, coords);
    
    jQuery.easing[str] = fn;
    if (name) { jQuery.easing[name] = fn };
    
    return fn;
  }
  
  // add standard css3 timing-functions
  addEasing("ease");
  addEasing("ease-in");
  addEasing("ease-out");
  addEasing("ease-in-out");
  addEasing("ease-in-out");
  addEasing("snap");
  
  jQuery.addEasing = addEasing;
})(jQuery);