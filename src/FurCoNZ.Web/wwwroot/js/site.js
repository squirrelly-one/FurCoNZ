// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

$(function () {
    // Tooltips are opt-in for performance reasons, so you must initialize them yourself. https://getbootstrap.com/docs/4.3/components/tooltips/
    $('[data-toggle="tooltip"]').tooltip()

    // Configure flatpickr as the default javascript date picker for all <input type="date"> fields
    flatpickr("input[type=date]", {
        dateFormat: "d/m/Y",
        altFormat: "Y-m-d",
        altInput: true,
        allowInput: true,
        // 🎉: https://github.com/flatpickr/flatpickr/issues/1551#issuecomment-522942541
        onClose(dates, currentdatestring, picker) {
            picker.setDate(picker.altInput.value, true, picker.config.altFormat)
        }
    });
});