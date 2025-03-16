var smallDisplayCommandsToggleButton = document.getElementById("small-display-commands-toggle-button");
var smallDisplayCommandsContainer = document.getElementById("small-display-commands-container");
var smallDisplayCommandsOverlayButton = document.getElementById("small-display-commands-overlay-button");

if (smallDisplayCommandsContainer.children.length == 0) {
    smallDisplayCommandsToggleButton.classList.add("hidden");
}

function toggleSmallCommandsDisplay() {
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