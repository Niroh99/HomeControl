function ToggleMainMenu() {
    let mainMenuComponents = document.getElementsByClassName("main-menu-component");

    for (mainMenuComponent of mainMenuComponents) {
        if (mainMenuComponent.classList.toggle("hidden")) {
            mainMenuButton.classList.remove("toggled");
        }
        else {
            mainMenuButton.classList.add("toggled");
        }
    }
}