var busyOverlay = document.getElementById("busy-overlay");

var defaultBusyMessage = "Loading...";

var busyMessage = document.getElementById("BusyMessage");

busyMessage.innerHTML = defaultBusyMessage;

function BusyWithMessage(message) {
    busyMessage.innerHTML = message;

    Busy();
}

function Busy() {
    busyOverlay.classList.remove("hidden");
}

function NotBusy() {
    busyMessage.innerHTML = defaultBusyMessage;

    busyOverlay.classList.add("hidden");
}