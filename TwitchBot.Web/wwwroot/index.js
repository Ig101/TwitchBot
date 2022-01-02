function sound() {
    var audio = new Audio('assets/34 - Blue Tide.mp3');
    audio.play();
}

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

connection.on("BlueCurrent", (user, message) => {
    console.log("BC");
    sound();
});

connection.onclose(async () => {
    await start();
});

// Start the connection.
start();