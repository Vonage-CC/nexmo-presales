function ShowPinCheck() {
    // hide the pin check window when the page first loads until the registration flow is triggered.
    $("#pinCheckWindow").hide();

    // check if the registration flow has been triggered
    var regStatus = $("#registrationStatus").val();
    if (regStatus == "started") {
        $("#pinCheckWindow").slideDown();
    }
}

$(document).ready(ShowPinCheck);