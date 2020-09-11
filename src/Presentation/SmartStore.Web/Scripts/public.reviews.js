
;
(function ($, window, document, undefined) {

    $(function () {

        $(".review-list").on("click", ".review-vote-link", function (e) {
            var el = $(this);
            var reviewId = el.parent().data("review-id");
            var href = el.parent().data("href");
            var isNo = el.is(".review-vote-link-no");

            setProductReviewHelpfulness(reviewId, isNo ? 'false' : 'true');

            function setProductReviewHelpfulness(reviewId, wasHelpful) {
                $.ajax({
                    cache: false,
                    type: "POST",
                    url: href,
                    data: { "productReviewId": reviewId, "washelpful": wasHelpful },
                    success: function (data) {
                        el.parent().bindData(data, { showFalsy: true });

                        if (data.Result) {
                            displayNotification(data.Result, data.Success ? "success" : "error");
                        }
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        alert('Failed to vote. Please refresh the page and try one more time.');
                    }
                });
            }
            return false;
        });

    });

})(jQuery, this, document);

