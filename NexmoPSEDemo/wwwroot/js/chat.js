
$(document).ready(function () {
    $('#chatControl').hide();

    setInterval(function () {
        $.get("https://nexmopsedemo.azurewebsites.net/messaging/sms/queue/next")
            .done(function (data) {
                if (data.length > 0 || data) {
                    // Parse the message object
                    var msg = JSON.parse(data);

                    // Build the message to display
                    var msisdn = $("<strong></strong>").text(msg.msisdn + " : ");
                    var text = $("<i></i>").text(msg.text);

                    // Display the message on screen
                    var chatTemp = document.createElement("div");
                    $(chatTemp).addClass("Vlt-badge");
                    chatTemp.append(msisdn[0], text[0]);
                    $('#chatTemplate').append(chatTemp, document.createElement("br"));
                }
            });
    }, 3000);
});

var message = { to: '', text: '' };

function setNumber() {
    var msisdn = document.getElementById("msisdn").value;

    if (msisdn == "" || msisdn == undefined) {
        alert('please enter a number');
        return;
    }
    else {
        message.to = msisdn;
        $('#chatControl').show();
        $('#chatTemplate').empty();
        $('#msisdn').val("");
        $('#message').text("You are now chatting with: " + message.to).css("font-weight", "Bold");
    }
}

function Send() {
    // Build the message to display
    message.text = document.getElementById("text").value;
    var from = $("<strong></strong>").text("You : ");
    var text = $("<i></i>").text(message.text);

    // Display the message on screen
    var chatTemp = document.createElement("div");
    $(chatTemp).addClass("Vlt-badge");
    $(chatTemp).addClass("Vlt-bg-green");
    chatTemp.append(from[0], text[0]);
    $('#chatTemplate').append(chatTemp, document.createElement("br"));

    // Send the SMS
    $.ajax({
        url: "https://nexmopsedemo.azurewebsites.net/messaging/sms/send",
        data: JSON.stringify(message),
        cache: false,
        type: 'POST',
        dataType: "json",
        contentType: 'application/json; charset=utf-8'
    });
    $("#text").val('');
}