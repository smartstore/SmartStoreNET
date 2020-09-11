
;
(function ($, window, document, undefined) {

    $(function () {

        $('.btn-votenow').on('click', function () {
            var btn = $(this);
            var id = btn.data("id");
            var pollAnswerId = $("input:radio[name=pollanswers-" + id + "]:checked").val();
            if (typeof (pollAnswerId) == 'undefined') {
                displayNotification(btn.data("message"), "error");
            }
            else {

                var box = $(this).closest(".poll-item");
                var throbber = box.data("throbber");
                if (!throbber) {
                    throbber = box.throbber({ small: true, white: true, show: false }).data("throbber");
                }
                throbber.show();

                $.ajax({
                    cache: false,
                    type: "POST",
                    url: btn.data("target"),
                    data: { "pollAnswerId": pollAnswerId },
                    success: function (data) {
                        if (data.error) {
                            displayNotification(data.error, "error");
                        }

                        if (data.html) {
                            box.replaceWith(data.html);
                        }
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        displayNotification('Failed to vote.', "error");
                        //voteProgress.hide();
                    },
                    complete: function () {
                        throbber.hide();
                    }
                });
            }
            return false;
        });

    });

})(jQuery, this, document);

