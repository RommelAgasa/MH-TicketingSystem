"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

// Disable the send button until the connection is established
document.getElementById("sendButton").disabled = true;

// Function to create the sender's message UI
function createSenderMessage(user, message, time, fileName, filePath) {

    const senderDiv = document.createElement("div");
    senderDiv.className = "chat-message-right pb-4";

    // Generate the download URL dynamically
    const downloadUrl = fileName && filePath
        ? `/Ticket/DownloadFile?filePath=${encodeURIComponent(filePath)}&fileName=${encodeURIComponent(fileName)}`
        : null;

    senderDiv.innerHTML = `
        <div>
            <div class="text-muted small text-nowrap mt-2">${time}</div>
        </div>
        <div class="flex-shrink-1 bg-light rounded py-2 px-3 ml-3">
            <div class="font-weight-bold mb-1">${user}</div>
            ${message}
            ${downloadUrl ? `<div><a href="${downloadUrl}">
                <i class="fas fa-arrow-circle-down"></i>
                ${fileName}</a></div>` : ""}
        </div>
    `;
    document.getElementById("messagesList").appendChild(senderDiv);
}


// Function to create the receiver's message UI
function createReceiverMessage(user, message, time, fileName, filePath) {
    const receiverDiv = document.createElement("div");
    receiverDiv.className = "chat-message-left pb-4";
    // Generate the download URL dynamically
    const downloadUrl = fileName && filePath
        ? `/Ticket/DownloadFile?filePath=${encodeURIComponent(filePath)}&fileName=${encodeURIComponent(fileName)}`
        : null;
    receiverDiv.innerHTML = `
        <div>
            <div class="text-muted small text-nowrap mt-2">${time}</div>
        </div>
        <div class="flex-shrink-1 bg-light rounded py-2 px-3 ml-3">
            <div class="font-weight-bold mb-1">${user}</div>
            ${message}
            ${downloadUrl ? `<div><a href="${downloadUrl}">
                <i class="fas fa-arrow-circle-down"></i>
                ${fileName}</a></div>` : ""}
        </div>
    `;
    document.getElementById("messagesList").appendChild(receiverDiv);
}

// Handle receiving messages
connection.on("ReceiveMessage", function (messageData) {
    const { user, message, file } = messageData;
    const currentTime = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

    // Only create a receiver message if the sender is not the current user
    if (user !== currentUser.name) {
        createReceiverMessage(user, message, currentTime, file?.fileName, file?.filePath);
    }
});

// Join the group when the connection starts
connection.start().then(async function () {
    const ticketId = parseInt(document.getElementById("ticketId").value);

    await connection.invoke("JoinTicketGroup", ticketId);
    document.getElementById("sendButton").disabled = false;
    //nsole.log(`Joined group for ticket ${ticketId}`);
}).catch(function (err) {
    console.error(err.toString());
});

document.getElementById("messageInput").addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
        event.preventDefault(); // Prevent the default "Enter" behavior, such as submitting a form
        document.getElementById("sendButton").click(); // Trigger the click event on the send button
    }
});

// Handle sending messages
document.getElementById("sendButton").addEventListener("click", async function (event) {
    event.preventDefault();

    const user = currentUser.name;
    const userId = currentUser.id;
    const message = document.getElementById("messageInput").value;
    const ticketId = parseInt(document.getElementById("ticketId").value);
    const fileInput = document.getElementById("file"); 

    // Check if the file is empty
    if (message.trim() === "" && fileInput.files.length === 0) {
        toastr.info("Message cannot be empty.");
        return;
    }

    let fileData = null;

    if (fileInput.files.length > 0) {
        // Upload the file to the server
        const file = fileInput.files[0];
        const formData = new FormData();
        formData.append("file", file);

        try {
            const response = await fetch("/FileUpload", {
                method: "POST",
                body: formData,
            });

            if (response.ok) {
                fileData = await response.json(); // Expecting a JSON response with { fileName, filePath }
            } else {
                toastr.error("Failed to upload file.");
                return;
            }
        } catch (err) {
            console.error(err);
            toastr.error("An error occurred during file upload.");
            return;
        }
    }

    // Construct the message data
    const messageData = {
        user,
        userId,
        message,
        file: fileData, // Include file info if available
    };

    try {
        await connection.invoke("SendMessageToTicket", ticketId, messageData);
        const currentTime = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        createSenderMessage("You", message, currentTime, fileData?.fileName, fileData?.filePath); // Update UI only after success
        document.getElementById("messageInput").value = ""; // Clear input after sending
        fileInput.value = ""; // Clear file input after sending
    } catch (err) {
        console.error(err.toString());
    }
});
