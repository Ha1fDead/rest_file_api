const template = document.createElement("template");
template.innerHTML = `
<div id="wrapper">
    <slot id="dialog-contents" class="dialog hidden" name="dialog-widget">Your content should go here</slot>
    <button id="dialog-open" type="button">View Directories</button>
    <button id="dialog-close" class="hidden" type="button">Close</button>
</div>`;

const style = document.createElement("style");
style.textContent = `
    .dialog {
        border: 1px solid green;
    }

    .hidden {
        display: none;
    }
`;

/**
 * A Dialog component. On a button click, will open a dialog with whatever the children of this component are
 * 
 * I decided not to implement true modal behavior, but here's a guide to do it: https://stackoverflow.com/questions/152975/how-do-i-detect-a-click-outside-an-element?noredirect=1&lq=1
 */
export default class DialogComponent extends HTMLElement {
    constructor() {
        super();

        const shadow = this.attachShadow({mode: "open"});
        shadow.appendChild(style.cloneNode(true));

        this._wrapper = template.content.cloneNode(true);
        shadow.appendChild(this._wrapper);

        const dialogButton = shadow.getElementById("dialog-open");
        dialogButton.addEventListener("click", this.HandleDialogOpen.bind(this), false);

        const dialogClose = shadow.getElementById("dialog-close");
        dialogClose.addEventListener("click", this.HandleDialogClose.bind(this), false);
    }

    HandleDialogClose(e) {
        const dialogContentWrapper = this.shadowRoot.getElementById("dialog-contents");
        dialogContentWrapper.classList.add("hidden");
        
        const dialogOpenButton = this.shadowRoot.getElementById("dialog-open");
        dialogOpenButton.classList.remove("hidden");

        const dialogClose = this.shadowRoot.getElementById("dialog-close");
        dialogClose.classList.add("hidden");
    }

    HandleDialogOpen(e) {
        const dialogContentWrapper = this.shadowRoot.getElementById("dialog-contents");
        dialogContentWrapper.classList.remove("hidden");

        const dialogOpenButton = this.shadowRoot.getElementById("dialog-open");
        dialogOpenButton.classList.add("hidden");

        const dialogClose = this.shadowRoot.getElementById("dialog-close");
        dialogClose.classList.remove("hidden");
    }
}

customElements.define("demo-dialog", DialogComponent);