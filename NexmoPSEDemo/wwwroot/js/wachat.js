var message = { to: '', text: '', type: '', template: false };
var fileMessage = { path: '', to: '', type: ''}
var url = "https://nexmopsedemo.azurewebsites.net/";
var currentDate = '';

$(document).ready(function () {
    $('#chatControl').hide();

    // Setting the url to match the environment where the app is running
    var currentUrl = window.location.hostname;
    if (currentUrl === "localhost") {
        url = "http://localhost:36802/"
    }

    getNextMessage(url);
    getStatusUpdate();
});

function getNextMessage(url) {
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
}

function getStatusUpdate() {
    setInterval(function () {
        $.get(url + "messaging/status")
            .done(function (data) {
                if (data.length > 0 || data) {
                    // Parse the message object
                    var status = JSON.parse(data);
                    var uuid = status.message_uuid;

                    var icon = "#" + uuid + " > img";
                    var statusIcon = document.createElement("img");
                    $(statusIcon).attr("width", "20");
                    $(statusIcon).attr("align", "left");

                    if (status.status === "submitted") {
                        $(statusIcon).attr("src", "/images/whatsapp/sent.png");
                        $(statusIcon).attr("title", "submitted");
                    }
                    else if (status.status === "delivered") {
                        $(statusIcon).attr("src", "/images/whatsapp/delivered.png");
                        $(statusIcon).attr("title", "delivered");
                    }
                    else if (status.status === "read") {
                        $(statusIcon).attr("src", "/images/whatsapp/read.png");
                        $(statusIcon).attr("title", "read");
                    }

                    if (status.status != "rejected") {
                        $(icon).replaceWith(statusIcon);
                    }
                }
            });
    }, 3000);
}

function setNumber() {
    var msisdn = document.getElementById("msisdn").value;

    if (msisdn == "" || msisdn == undefined) {
        alert('please enter a number');
        return;
    }
    else {
        message.to = msisdn;
        fileMessage.to = msisdn;
        $('#chatControl').show();
        $('#chatTemplate').empty();
        $('#msisdn').val("");
        $('#message').text("You are now chatting with: " + message.to).css("font-weight", "Bold");
    }
}

function Send() {
    message.text = document.getElementById("text").value;
    message.type = "text";

    // Send the WhatsApp message
    $.ajax({
        url: url + "messaging/wa/send",
        data: JSON.stringify(message),
        cache: false,
        type: 'POST',
        dataType: "json",
        contentType: 'application/json; charset=utf-8'
    })
        .done(function (result) {

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

            var status = document.createElement("div");
            $(status).attr("id", result.message_uuid);
            var pending = document.createElement("img");
            $(pending).attr("src", "/images/whatsapp/pending.png");
            $(pending).attr("title", "pending");
            $(pending).attr("width", "20");
            $(pending).attr("align", "left");
            $(status).append(pending);

            $(badgeWrapper).append(chatBadge);
            $(badgeWrapper).append(chatTime);

            $(chatWrapper).append(badgeWrapper);
            $(chatWrapper).append(status);

            $('#chatTemplate').append(chatWrapper, document.createElement("br"));

            // Empty the input field
            $("#text").val('');
        })
        .fail(function (e) {
            alert("An error has occured. Please try again later: " + e.message);
        });
}

function sendFile() {
    var selected = $("input:checked");

    if ($(selected).val() === "image") {
        fileMessage.path = "wa-file-upload.jpg";
    }
    else if ($(selected).val() === "file") {
        fileMessage.path = "wa-file-upload.pdf";
    }
    else if ($(selected).val() === "audio") {
        fileMessage.path = "wa-file-upload.mp3";
    }

    fileMessage.type = selected.val();

    // Send the WhatsApp file
    $.ajax({
        url: url + "messaging/wa/file/send",
        data: JSON.stringify(fileMessage),
        cache: false,
        type: 'POST',
        dataType: "json",
        contentType: 'application/json; charset=utf-8'
    })
        .done(function (result) {

            var from = $("<i></i>").text("You : ");
            var text = $("<strong></strong>").text(message.text);
            var date = new Date();
            var time = date.toLocaleTimeString('fr-fr');

            var chatTime = document.createElement("div");
            $(chatTime).addClass("Vlt-badge");
            $(chatTime).addClass("Vlt-bg-blue");
            $(chatTime).text(time);

            // Display the file on screen
            var chatBadge = document.createElement("div");
            $(chatBadge).addClass("Vlt-badge");
            $(chatBadge).addClass("Vlt-bg-orange");
            chatBadge.append(from[0], text[0]);

            var chatWrapper = document.createElement("div");
            $(chatWrapper).addClass("Vlt-right");

            var badgeWrapper = document.createElement("div");
            $(badgeWrapper).addClass("Vlt-badge-combined");

            var status = document.createElement("div");
            $(status).attr("id", result.message_uuid);
            var pending = document.createElement("img");
            $(pending).attr("src", "/images/whatsapp/pending.png");
            $(pending).attr("title", "pending");
            $(pending).attr("width", "20");
            $(pending).attr("align", "left");
            $(status).append(pending);

            $(badgeWrapper).append(chatBadge);
            $(badgeWrapper).append(chatTime);

            $(chatWrapper).append(badgeWrapper);
            $(chatWrapper).append(status);

            $('#chatTemplate').append(chatWrapper, document.createElement("br"));

            // Empty the input field
            $("#text").val('');
        })
        .fail(function (e) {
            alert("An error has occured. Please try again later: " + e.message);
        });
}
