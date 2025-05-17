(function ($) {
    $(function () {
        // Initialize Materialize CSS components
        $('.sidenav').sidenav();
        $('.parallax').parallax();
        // M.updateTextFields(); // Optional: Initialize text fields globally

        // Prevent browser back and forward buttons
        if (window.history && window.history.pushState) {
            window.history.pushState('forward', '', window.location.href);
            $(window).on('popstate', function (e) {
                window.history.pushState('forward', '', window.location.href);
                e.preventDefault();
            });
        }

        // Prevent right-click on entire window
        $(window).on("contextmenu", function () {
            return false;
        });
    });
})(jQuery);
