// Global AJAX Anti-Forgery Configuration
$(function () {
    $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
        if (options.type.toUpperCase() === "POST" || options.type.toUpperCase() === "PUT" || options.type.toUpperCase() === "DELETE") {
            var token = $('input[name="__RequestVerificationToken"]').val();
            if (token) {
                jqXHR.setRequestHeader('RequestVerificationToken', token);
            }
        }
    });

    // Auto-dismiss alerts after 6 seconds
    setTimeout(function () {
        $('.alert-dismissible').fadeOut('slow');
    }, 6000);
});
