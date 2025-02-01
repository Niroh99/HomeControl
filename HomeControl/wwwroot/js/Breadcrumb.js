var breadcrumbs = document.getElementsByName("Breadcrumb");

//if (breadcrumbs.length == 0) return;

var resizeObserver = new ResizeObserver((entries) => {
    for (entry of entries) {
        updateBreadcrumb(entry.target);
    }
});

for (breadcrumb of breadcrumbs) {
    updateBreadcrumb(breadcrumb);

    resizeObserver.observe(breadcrumb);
}

function updateBreadcrumb(breadcrumb) {
    if (breadcrumb == undefined) return;
    if (breadcrumb == null) return;

    let breadcrumbDropdownButton = breadcrumb.children[0];

    breadcrumbDropdownButton.classList.add("hidden");

    let breadcrumbChildren = breadcrumb.children[1].children;

    if (breadcrumbChildren.length == 0) return;

    for (let child of breadcrumbChildren) child.classList.remove("hidden");

    let firstChild = breadcrumbChildren[0];

    let firstChildOffset = firstChild.offsetTop;

    let lastChild = breadcrumbChildren[breadcrumbChildren.length - 1];

    let lastChildOffset = lastChild.offsetTop;

    if (firstChildOffset == lastChildOffset) return;

    breadcrumbDropdownButton.classList.remove("hidden");

    let i = 0;

    while (firstChildOffset != lastChildOffset && i < breadcrumbChildren.length - 1) {
        breadcrumbChildren[i].classList.add("hidden");

        firstChildOffset = breadcrumbChildren[i + 1].offsetTop;
        lastChildOffset = breadcrumbChildren[breadcrumbChildren.length - 1].offsetTop;

        i += 1;
    }
}