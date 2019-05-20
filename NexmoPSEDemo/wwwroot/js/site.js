// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function fillTextareaWithTemplate(textarea, contentId) {
    var content = $("#" + contentId + " > p").html();
    $("#" + textarea).html(content);

    if (contentId == "wa-welcome-template") {
        $("#template").val("true");
    }
};

function toggleStatusBoxes() {
    // only show callouts with information
    if ($("#infoCallout > div > div > pre").text() == "")
        $("#infoCallout").hide();
    else
        $("#infoCallout").slideDown();

    if ($("#warningCallout > div > div").text() == "")
        $("#warningCallout").hide();
    else
        $("#warningCallout").slideDown();

    if ($("#errorCallout > div > div").text() == "")
        $("#errorCallout").hide();
    else
        $("#errorCallout").slideDown();
}

function generatePin() {
    var randomPin = "";
    var length = Math.floor(Math.random() * (11 - 4)) + 4;

    var char_list = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    for (var i = 0; i < length; i++) {
        randomPin += char_list.charAt(Math.floor(Math.random() * char_list.length));
    }

    $("#pin").val(randomPin);
    event.preventDefault();
}

$(document).ready(toggleStatusBoxes);
