// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function fillTextareaWithTemplate(textarea, contentId) {
    var content = $("#" + contentId + " > p").html();
    $("#" + textarea).html(content);
};

function toggleStatusBoxes() {
    // only show callouts with information
    if ($("#infoCallout > div > div").text() == "")
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

$(document).ready(toggleStatusBoxes);
