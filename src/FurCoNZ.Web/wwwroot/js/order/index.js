function dateOfBirthChanged(e) {
    var modelId = $(e.target).data("order-id");
    var dateOfBirthString = e.target.value;
    var age = getAgeAtDate(dateOfBirthString, '2020-01-30'); // TODO: Get date from config

    if (age < 18 && age >= 16) {
        $("#attendee-" + modelId + "-under18Notice").removeClass('d-none');
        $("#attendee-" + modelId + "-under16Notice").addClass('d-none');
    } else if (age < 16) {
        $("#attendee-" + modelId + "-under18Notice").addClass('d-none');
        $("#attendee-" + modelId + "-under16Notice").removeClass('d-none');
    } else {
        $("#attendee-" + modelId + "-under18Notice").addClass('d-none');
        $("#attendee-" + modelId + "-under16Notice").addClass('d-none');
    }
}

function getAgeAtDate(dateOfBirthString, dateOfEventString) {
    var dateOfBirth = new Date(dateOfBirthString);
    var dateOfEvent = new Date(dateOfEventString);
    var age = dateOfEvent.getFullYear() - dateOfBirth.getFullYear();
    var m = dateOfEvent.getMonth() - dateOfBirth.getMonth();
    if (m < 0 || (m === 0 && dateOfEvent.getDate() < dateOfBirth.getDate())) {
        age--;
    }
    return age;
}

function beginStripPayment(e) {
    const stripe = Stripe($(e.target).data("stripe-key"));
    const sessionId = $(e.target).data("session-id");

    stripe.redirectToCheckout({
        sessionId: sessionId,
    });
}

$(function () {
    $(".order-dob-field").on("change", dateOfBirthChanged);
    $("#payWithStripeButton").on("click", beginStripPayment);
});
