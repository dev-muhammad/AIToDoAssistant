const connection = new signalR.HubConnectionBuilder().withUrl("/chathub").build();

connection.on("ReceiveMessage", function (message) {
    const msg = document.createElement("div");
    msg.classList.add('message', 'assistant');
    msg.innerHTML = `
        <div class="avatar"><i class="fas fa-robot"></i></div>
        <div class="content">${message}</div>
    `;
    document.getElementById("messagesList").appendChild(msg);
    document.getElementById("messagesList").scrollTop = document.getElementById("messagesList").scrollHeight;
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

function sendMessage() {
    const message = document.getElementById("messageInput").value;
    if (message.trim() === "") {
        return;
    }

    const msg = document.createElement("div");
    msg.classList.add('message', 'user');
    msg.innerHTML = `
        <div class="avatar"><i class="fas fa-user"></i></div>
        <div class="content">${message}</div>
    `;
    document.getElementById("messagesList").appendChild(msg);
    document.getElementById("messageInput").value = '';
    document.getElementById("messagesList").scrollTop = document.getElementById("messagesList").scrollHeight;

    connection.invoke("SendMessage", message).catch(function (err) {
        return console.error(err.toString());
    });
}

document.getElementById("messageInput").addEventListener("keypress", function (e) {
    if (e.key === "Enter") {
        sendMessage();
    }
});