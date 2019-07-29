function dateOfBirthChanged(e, modelId) {
    var dateOfBirthString = e.value;
    var age = getAgeAtDate(dateOfBirthString, '2020-03-01'); // TODO: Get date from config

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