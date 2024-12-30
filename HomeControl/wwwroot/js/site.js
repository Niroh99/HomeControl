var root = document.querySelector(':root');

var header = document.getElementsByTagName("header")[0];

document.addEventListener("load", (e) => {
    SetHeaderHeight(header.getComputedStyle().height);
});

var mainMenuButton = document.getElementById("main-menu-button");

var resizeObserver = new ResizeObserver((entries) => {
    let entry = entries.pop();

    let height = entry.borderBoxSize[0].blockSize;

    SetHeaderHeight(height);
});

resizeObserver.observe(header);

function SetHeaderHeight(height) {
    root.style.setProperty("--headerHeight", `${height}px`);
}

SetScrollY(0);

document.addEventListener("scroll", (e) => SetScrollY(window.scrollY));

function SetScrollY(scrollY) {
    root.style.setProperty("--scrollY", `${scrollY}px`);
}