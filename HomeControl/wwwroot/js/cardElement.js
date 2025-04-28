class CardElement extends HTMLElement {
    static observedAttributes = ["header", "info-text", "error-text"];

    constructor() {
        super();
    }

    #header = "";
    #headerElement = null;
    #infoText = "";
    #infoTextElement = null;
    #errorText = "";
    #errorTextElement = null;

    get header() {
        return this.#header;
    }

    set header(value) {
        this.#header = value;
        this.#onHeaderChanged();
    }

    get infoText() {
        return this.#infoText;
    }

    set infoText(value) {
        this.#infoText = value;
        this.#onInfoTextChanged();
    }

    get errorText() {
        return this.#errorText;
    }

    set errorText(value) {
        this.#errorText = value;
        this.#onErrorTextChanged();
    }

    connectedCallback() {
        this.attachHeaderElement(this.#firstChildElementByNameRecursive(this, "Header"));
        this.attachInfoTextElement(this.#firstChildElementByNameRecursive(this, "InfoText"));
        this.attachErrorTextElement(this.#firstChildElementByNameRecursive(this, "ErrorText"));
    }

    attachHeaderElement(headerElement) {
        if (this.#headerElement != null) this.#headerElement.innerHTML = "";

        this.#headerElement = headerElement;

        this.#onHeaderChanged();
    }

    attachInfoTextElement(infoTextElement) {
        if (this.#infoTextElement != null) this.#infoTextElement.innerHTML = "";

        this.#infoTextElement = infoTextElement;

        this.#onInfoTextChanged();
    }

    attachErrorTextElement(errorTextElement) {
        if (this.#errorTextElement != null) this.#errorTextElement.innerHTML = "";

        this.#errorTextElement = errorTextElement;

        this.#onErrorTextChanged();
    }

    disconnectedCallback() {

    }

    attributeChangedCallback(name, oldValue, newValue) {
        switch (name) {
            case "header": this.header = newValue; break;
            case "info-text": this.infoText = newValue; break;
            case "error-text": this.errorText = newValue; break;
        }
    }

    #firstChildElementByNameRecursive(parent, name) {
        if (parent.children == undefined) return null;

        let element = parent.children[name];

        if (element != undefined) return element;

        for (let child of parent.children) {
            let childElementByName = this.#firstChildElementByNameRecursive(child, name);

            if (childElementByName != null) return childElementByName;
        }

        return null;
    }

    #onHeaderChanged() {
        if (this.#headerElement == null) return;

        this.#headerElement.innerHTML = this.#header;
    }

    #onInfoTextChanged() {
        if (this.#infoTextElement == null) return;

        this.#infoTextElement.innerHTML = this.#infoText;
    }

    #onErrorTextChanged() {
        if (this.#errorTextElement == null) return;

        this.#errorTextElement.innerHTML = this.#errorText;
    }
}

customElements.define("card-element", CardElement);