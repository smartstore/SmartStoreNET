
var Hacks = {

    Telerik: {
        // TODO: temp > Handle k-textbox with an EditorTemplate
        handleTextBox: function (el) {
            //el.addClass("k-textbox");
        },

        // TODO: temp > Handle Button with MVC
        handleButton: function (el) {
            el.removeClass("t-button").addClass("btn");
            el.each(function () {
                var btn = $(this);
                if (btn.hasClass("t-grid-add")) {
                    btn.addClass("btn-warning").prepend('<i class="icon-plus"></i>&nbsp;');
                }
            });

        }
    },

    Kendo: {

        // Kendo doesn't set content elements display to block
        // when window is opened
        windowContentVisibility: function (elWindow) {
            var kendoWindow = elWindow.data('kendoWindow');

            kendoWindow.bind("open", function (e) {
                elWindow.css("display", "block");
            });

            kendoWindow.bind("deactivate", function (e) {
                elWindow.css("display", "none");
            });
        },

        // Temporary hack to bypass a kendo grid bug, which
        // attempts to sort any column, whether disabled or not.
        fixGridSorting: function (elGrid, sortable) {
            // if 'sortable' is true, all columns NOT in arglist are not sortable,
            // if false, all columns NOT in arglist are sortable
            var colNames;
            if (_.isArray(arguments[2]))
                colNames = arguments[2];
            else
                colNames = _.toArray(arguments).slice(2);

            if (colNames.length > 0) {
                elGrid.find(".k-header .k-link").on("click", function (e) {
                    var clickedColumnName = $(this).parent().data("field");

                    var match = _.any(colNames, function (val) {
                        return (val == clickedColumnName);
                    });

                    if ((sortable && !match) || (!sortable && match)) {
                        e.stopPropagation();
                    }

                });
            }
        }

    }

}