var busyOverlay = document.getElementById("busy-overlay");

var defaultBusyMessage = "Loading...";

var busyMessage = document.getElementById("BusyMessage");

busyMessage.innerHTML = defaultBusyMessage;

function BusyWithMessage(message) {
    busyMessage.innerHTML = message;

    busy();
}

function busy() {
    busyOverlay.classList.remove("hidden");
}

function notBusy() {
    busyMessage.innerHTML = defaultBusyMessage;

    busyOverlay.classList.add("hidden");
}