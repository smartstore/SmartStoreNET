$(function () {
    $('.fa-icon-chooser').each(function (i, select) {
        var map = {};
        $(select).data('icons', map);

        // Add icon data of selected icon (intial option) to the map very early
        var selectedOption = $(select).find('option[data-icon][selected]');
        if (selectedOption.length) {
            var icon = selectedOption.data('icon');
            map[icon.id] = icon;
        }

        $(select).select2({
            escapeMarkup: function (markup) { return markup; }, // let our custom formatter work
            minimumInputLength: 0,
            allowClear: true,
            templateResult: formatIcon,
            templateSelection: formatIconSelection,
            tags: true,
            createTag: function (params) {
                var term = $.trim(params.term);

                if (term === '') {
                    return null;
                }

                return {
                    id: term,
                    text: term,
                    isCustom: true
                };
            },
            ajax: {
                url: function (params) {
                    return $(select).data('explorer-url');
                },
                global: false,
                type: "POST",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return {
                        term: params.term,
                        selected: $(select).val(),
                        page: params.page
                    };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;

                    return {
                        results: data.results,
                        pagination: data.pagination
                    };
                },
                cache: true
            }
        });

        function formatIconSelection(icon, el) {
            return formatIcon(icon, el, true);
        }

        function formatIcon(icon, el, isSelection) {
            if (icon.loading || !icon.id)
                return icon.text;

            var html = ['<span class="choice-item">'];

            var variants = [];
            var iconClass = '';

            if (icon.element && $(icon.element).data('icon')) {
                // Option is pregenerated
                icon = $(icon.element).data('icon');
            }

            map[icon.id] = icon;

            if (icon.isCustom) {
                // Has been set in "createTag" function or server-side (if IsPro == true)
                html.push('<i class="fas fa-question text-warning fa-fw mr-2 fs-h6" />');
            }
            else {
                if (icon.isBrandIcon) {
                    variants.push("fab fa-" + icon.text);
                }
                else if (icon.hasRegularStyle) {
                    variants.push("fas fa-" + icon.text);
                    variants.push("far fa-" + icon.text);
                }
                else {
                    variants.push("fas fa-" + icon.text);
                }

                var len = (isSelection ? 0 : 2) || variants.length;

                for (i = 0; i < len; i++) {
                    iconClass = (i < variants.length ? variants[i] + " " : "far ") + "fa-fw mr-2 fs-h6";
                    html.push('<i class="' + iconClass + '" />');
                }
            }

            html.push(icon.text);
            html.push('</span>');

            return $(html.join(''));
        }
    });
});