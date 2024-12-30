var dialogOverlay = document.getElementById("dialog-overlay");

function showDialog(dialog) {
    dialogOverlay.classList.remove("hidden");

    dialog.show();
}

function closeDialog(dialog) {
    dialogOverlay.classList.add("hidden");

    dialog.close();
}

function closeAllDialogs() {
    let dialogs = document.getElementsByTagName("dialog");

    for (dialog of dialogs) {
        dialog.close();
    }

    dialogOverlay.classList.add("hidden");
}