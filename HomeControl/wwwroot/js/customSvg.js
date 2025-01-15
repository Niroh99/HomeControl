class customSvg extends HTMLElement {
    static observedAttributes = ["style", "class"];

    constructor() {
        super();
    }

    getPathInfo(pathName) {
        switch (pathName) {
            case "arrow-down":
                return {
                    strokeWidth: "1px",
                    fill: false,
                    data: "M 0 0 L 5 5 L 10 0"
                }
            case "arrow-up":
                return {
                    strokeWidth: "1px",
                    fill: false,
                    data: "M 0 6 L 5 1 L 10 6"
                }
            case "arrow-right":
                return {
                    strokeWidth: "1px",
                    fill: false,
                    data: "M 0 0 L 5 5 L 0 10"
                }
            case "hamburger":
                return {
                    strokeWidth: "1px",
                    fill: false,
                    data: "M 0 1 L 12 1 M 0 6 L 12 6 M 0 11 L 12 11"
                }
            case "directory":
                return {
                    strokeWidth: "1px",
                    fill: false,
                    data: "M 1 1 L 8 1 L 10 4 L 17 4 L 17 15 L 1 15 Z"
                }
        }
    }

    updatePathInfo(style) {

        if (this.svg == undefined || this.path == undefined) return;

        let pathInfo = this.getPathInfo(style.getPropertyValue("--path"));

        let width = style.width;
        let height = style.height;

        this.svg.style.width = width;
        this.svg.style.height = height;
        this.svg.style.display = "inherit";

        let color = style.color;

        this.path.setAttribute("stroke", color);

        if (pathInfo.fill) {
            this.path.setAttribute("fill", this.getAttribute(color));
        }
        else {
            this.path.setAttribute("fill", "transparent");
        }

        if (pathInfo.strokeWidth != null) {
            this.path.setAttribute("stroke-width", pathInfo.strokeWidth);
        }

        this.path.setAttribute("d", pathInfo.data);
    }

    connectedCallback() {
        this.shadow = this.attachShadow({ mode: 'open' });

        this.shadow.innerHTML =
            `<svg id="svg">
                <path id="path"></path>
             </svg>
            `;

        this.svg = this.shadow.getElementById("svg");

        this.path = this.shadow.getElementById("path");

        this.updatePathInfo(window.getComputedStyle(this));

        this.shadow.appendChild(this.svg);
    }

    attributeChangedCallback(name, oldValue, newValue) {
        switch (name) {
            case "style":
            case "class":
                this.updatePathInfo(window.getComputedStyle(this));
                break;
        }
    }
}

customElements.define("custom-svg", customSvg);