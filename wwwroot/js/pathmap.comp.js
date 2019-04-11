import {html, render} from 'https://unpkg.com/lit-html?module';
import FileService from './file.service.js';


// This uses lit-html (see readme)
const template = document.createElement("template");
template.innerHTML = `<div id="wrapper">
</div>`;

const style = document.createElement("style");
style.textContent = `

`;

export default class PathMapComponent extends HTMLElement {


    constructor() {
        super();

        this.fileService = new FileService();

        const shadow = this.attachShadow({mode: "open"});
        shadow.appendChild(style.cloneNode(true));

        this._wrapper = template.content.cloneNode(true);
        shadow.appendChild(this._wrapper);
        this._update();
    }

    async _update() {
        let directory = await this.fileService.GetDirectory(this.fileService.GetCurrentRelativePath());

        // need to split the string but return the entire string from the split
        let pathTemplate = (currentPath) => html`
            <a href="/">Home</a> 
            ${this.splitStringIntoParts(currentPath, "/").map((subpath) => html`
                / <a href="${subpath.prev}">${subpath.part}</a>
            `)}
        `;

        render(pathTemplate(directory.relativePath), this.shadowRoot.getElementById("wrapper"));
    }

    splitStringIntoParts(str, delimiter) {
        let split = str.split(delimiter);
        let parts = [];
        for (let x = 0; x < split.length; x++) {
            let previous = `/${split.slice(0, x + 1).join("/")}`;
            parts[x] = {
                prev: previous,
                part: split[x]
            };
        }

        return parts;
    }
}

customElements.define("demo-path-map", PathMapComponent);