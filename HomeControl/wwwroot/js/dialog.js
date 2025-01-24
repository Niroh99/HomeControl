var dialogOverlay = document.getElementById("dialog-overlay");

function showDialogById(dialogId) {
    let dialog = document.getElementById(dialogId);

    showDialog(dialog);
}

function showDialog(dialog) {
    dialogOverlay.classList.remove("hidden");

    dialog.show();
}

function closeDialogById(dialogId) {
    let dialog = document.getElementById(dialogId);

    closeDialog(dialog);
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