var message = { to: '', text: '' };
var url = "https://nexmopsedemo.azurewebsites.net/";
var currentDate = '';

$(document).ready(function () {
    $('#chatControl').hide();

    // Setting the url to match the environment where the app is running
    var currentUrl = window.location.hostname;
    if (currentUrl === "localhost") {
        url = "http://localhost:36802/"
    }

    setInterval(function () {
        $.get(url + "messaging/wa/queue/next")
            .done(function (data) {
                if (data.length > 0 || data) {
                    // Parse the message object
                    var msg = JSON.parse(data);

                    // Parse the timestamp
                    var date = msg.timestamp.substring(0, 10);
                    if (date != currentDate) {
                        currentDate = date;

                        var chatTemp = document.createElement("div");
                        $(chatTemp).attr("align", "center");
                        chatTemp.append(date);
                        $('#chatTemplate').append(chatTemp, document.createElement("br"));
                    }

                    var time = msg.timestamp.substring(11);
                    var chatTime = document.createElement("div");
                    $(chatTime).addClass("Vlt-badge");
                    $(chatTime).addClass("Vlt-bg-orange");
                    $(chatTime).text(time);

                    // Build the message to display
                    var msisdn = $("<i></i>").text(msg.from.Number + " : ");
                    var text = $("<strong></strong>").text(msg.message.Content.Text);

                    // Display the message on screen
                    var chatWrapper = document.createElement("div");
                    $(chatWrapper).addClass("Vlt-right");

                    var badgeWrapper = document.createElement("div");
                    $(badgeWrapper).addClass("Vlt-badge-combined");

                    var chatBadge = document.createElement("div");
                    $(chatBadge).addClass("Vlt-badge");
                    chatBadge.append(msisdn[0], text[0]);

                    $(badgeWrapper).append(chatBadge);
                    $(badgeWrapper).append(chatTime);
                    $(chatWrapper).append(badgeWrapper);
                    $('#chatTemplate').append(chatWrapper, document.createElement("br"));
                }
            });
    }, 3000);
});

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
    message.text = document.getElementById("text").value;

    // Send the SMS
    $.ajax({
        url: url + "messaging/wa/send",
        data: JSON.stringify(message),
        cache: false,
        type: 'POST',
        dataType: "json",
        contentType: 'application/json; charset=utf-8'
    })
    .done(function (result) {
        // Build the message to display
        var from = $("<i></i>").text("You : ");
        var text = $("<strong></strong>").text(message.text);
        var date = new Date();
        var time = date.toLocaleTimeString('fr-fr');

        var chatTime = document.createElement("div");
        $(chatTime).addClass("Vlt-badge");
        $(chatTime).addClass("Vlt-bg-blue");
        $(chatTime).text(time);

        // Display the message on screen
        var chatBadge = document.createElement("div");
        $(chatBadge).addClass("Vlt-badge");
        $(chatBadge).addClass("Vlt-bg-orange");
        chatBadge.append(from[0], text[0]);

        var chatWrapper = document.createElement("div");
        $(chatWrapper).addClass("Vlt-right");

        var badgeWrapper = document.createElement("div");
        $(badgeWrapper).addClass("Vlt-badge-combined");

        $(badgeWrapper).append(chatBadge);
        $(badgeWrapper).append(chatTime);
        $(chatWrapper).append(badgeWrapper);
        $('#chatTemplate').append(chatWrapper, document.createElement("br"));

        // Empty the input field
        $("#text").val('');
    })
    .fail(function (e) {
        alert("An error has occured. Please try again later: " + e.message);
    });
}