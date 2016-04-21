/*
*  Project: SmartStore column equalizer 
*  Author: Murat Cakir, SmartStore AG
*/
;
(function ($, window, document, undefined) {
    
    function Equalizer(cols, options) {

        var curTallest = 0,
            curRowStart = 0,
            curParent = null, // the current offset parent (as element, not jq)
            rowCols = [],
            // HashTable<string, { tallest: int, elements[] }> : string = part name, obj = all parts (jq)
            colParts = {};

        function reset() {
            rowCols = []; // empty the array
            colParts = {}; // empty parts
            curRowStart = 0;
            curParent = null,
            curTallest = 0;
        }

        function getOriginalHeight(el, resize) {
            // if the height has changed, return the originalHeight
            if (resize) {
                return el.height();
            }
            var h = el.data("original-height");
            return parseFloat((h === undefined) ? el.height() : h);
        }

        function setHeight(el, newHeight) {
            // set the height to something new, but remember the original height in case things change
            var h = el.data("original-height");
            var valign = el.data("equalized-valign");
            el.data("original-height", (h === undefined) ? parseFloat(el.css("height")) : h);
            el.css("min-height", newHeight);
            if (valign) {
                el.css({ lineHeight: newHeight + "px", verticalAlign: "middle" });
            }
        }
        
        function equalize(resize) {

            // find the tallest column in the row, and set the heights 
            // of all of the columns to match it.
            cols.each(function (index) {

                var deep, parts, part;

                var col = $(this);
                
                deep = (options.deep === undefined || options.deep === null) ? col.data("equalized-deep") : options.deep;

                var applyHeight = function () {
                    
                    if (rowCols.length < 2)
                        return; // useless to equalize a 1-col row
                    
                    for (curCol = 0; curCol < rowCols.length ; ++curCol) {

                        if (deep) {
                            // set heights of all parts in the column
                            // (but not the column itself)
                            $.each(colParts, function (name, val) {
                                // iterate all parts
                                $.each(val.elements, function () {
                                    // and set height of all part elements in each col to the max
                                    setHeight($(this), val.tallest);
                                });
                            });
                        }
                        else {
                            // set height of the columns only
                            setHeight(rowCols[curCol], curTallest);
                        }

                    }
                }

                if (curRowStart != col.position().top || !col.offsetParent().is(curParent)) {
                    // we just came to a new row.  
                    // Apply all the heights on the (previous) completed row
                    applyHeight();
                    // set the variables for the new row
                    rowCols.length = 0; // empty the array
                    colParts = {}; // empty parts
                    curRowStart = col.position().top;
                    curParent = col.offsetParent()[0];
                    curTallest = getOriginalHeight(col, resize);
                    deep = (options.deep === undefined || options.deep === null) ? col.data("equalized-deep") : options.deep;
                    rowCols.push(col);

                    // determine deep parts (first col in row is enough)
                    if (deep) {
                        parts = col.find("[data-equalized-part]");
                        parts.each(function () {
                            part = $(this);
                            var name = part.data("equalized-part");
                            if (name) { // ensure it's not empty
                                colParts[name] = { tallest: getOriginalHeight(part, resize), elements: [part] };
                            }
                        });
                    }
                }
                else {

                    // another col on the current row. 
                    // Add it to the list and check if it's taller
                    rowCols.push(col);

                    if (deep) {
                        // find all parts in this sibling col
                        $.each(colParts, function (name, val) {
                            part = col.find("[data-equalized-part=" + name + "]");
                            if (part.length == 1) {
                                val.tallest = Math.max(getOriginalHeight(part, resize), val.tallest);;
                                val.elements.push(part);
                            }
                        });
                    }
                    else {
                        curTallest = Math.max(getOriginalHeight(col, resize), curTallest);
                    }

                }
                // do the last row
                applyHeight();

            });

        }

        // do the work not before all images contained within
        // all columns are loaded, otherwise real heights
        // cannot be determined reliably.
        $.preload(cols.find("img"), equalize);

        if (options.responsive) {
            EventBroker.subscribe("page.resized", function (data) {
                //console.log("Must equalize columns");
                reset();
                equalize(true);
            });
        }

        // for jQuery chaining
        return cols;
    }

    var defaults = {
        // data-equalized-deep (row-wide)
        deep: undefined,
        responsive: undefined
    };

    $.fn.equalizeColumns = function (options) {
        return new Equalizer(this, $.extend({}, defaults, options));
    }

})(jQuery, window, document);