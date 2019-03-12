
var apiKey = "46285232";
var sessionId = "2_MX40NjI4NTIzMn5-MTU1MjM5NDE4NzM1MH5qN1V6OTNVSitWd3BpNi9aSHh5RitwRUF-fg";
var token = "T1==cGFydG5lcl9pZD00NjI4NTIzMiZzaWc9YzNiMWYwOGEyOGVjOTRjMDRmY2RhYTM4NGRkZTc2OTE5Y2EyZTUzNjpzZXNzaW9uX2lkPTJfTVg0ME5qSTROVEl6TW41LU1UVTFNak01TkRFNE56TTFNSDVxTjFWNk9UTlZTaXRXZDNCcE5pOWFTSGg1Uml0d1JVRi1mZyZjcmVhdGVfdGltZT0xNTUyMzk0MjE2Jm5vbmNlPTAuNzU1NDAxNjMzNDMzMTM0NCZyb2xlPXB1Ymxpc2hlciZleHBpcmVfdGltZT0xNTUyNDgwNjEzJmluaXRpYWxfbGF5b3V0X2NsYXNzX2xpc3Q9";

// Handling all of our errors here by alerting them
function handleError(error) {
    if (error) {
        alert(error.message);
    }
}

function initializeSession() {
    var session = OT.initSession(apiKey, sessionId);

    // Subscribe to a newly created stream
    session.on('streamCreated', function (event) {
        session.subscribe(event.stream, 'subscriber', {
            insertMode: 'append',
            width: '100%',
            height: '100%'
        }, handleError);
    });

    // Create a publisher
    var publisher = OT.initPublisher('publisher', {
        insertMode: 'append',
        width: '100%',
        height: '100%'
    }, handleError);

    // Connect to the session
    session.connect(token, function (error) {
        // If the connection is successful, publish to the session
        if (error) {
            handleError(error);
        } else {
            session.publish(publisher, handleError);
        }
    });
}

initializeSession();