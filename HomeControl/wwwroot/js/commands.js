function toggleSmallCommandsDisplay() {
    let smallDisplayCommandsToggleButton = document.getElementById("small-display-commands-toggle-button");
    let smallDisplayCommandsContainer = document.getElementById("small-display-commands-container");
    let smallDisplayCommandsOverlayButton = document.getElementById("small-display-commands-overlay-button");

    if (smallDisplayCommandsToggleButton.isChecked == undefined) smallDisplayCommandsToggleButton.isChecked = false;

    if (smallDisplayCommandsToggleButton.isChecked) {
        smallDisplayCommandsToggleButton.isChecked = false;

        smallDisplayCommandsContainer.classList.add("hidden");
        smallDisplayCommandsOverlayButton.classList.add("hidden");
    }
    else {
        smallDisplayCommandsToggleButton.isChecked = true;

        smallDisplayCommandsContainer.classList.remove("hidden");
        smallDisplayCommandsOverlayButton.classList.remove("hidden");
    }
}