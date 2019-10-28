// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

$(function () {
    // Tooltips are opt-in for performance reasons, so you must initialize them yourself. https://getbootstrap.com/docs/4.3/components/tooltips/
    $('[data-toggle="tooltip"]').tooltip()

    // Configure flatpickr as the default javascript date picker for all <input type="date"> fields
    flatpickr("input[type=date]", {
        dateFormat: "Y-m-d",
        allowInput: true
    });
});