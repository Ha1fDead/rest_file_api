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

    HandleDelete(e, relativePath, fileName) {
        this.fileService.Delete(relativePath, fileName);
    }

    async _update() {
        let directory = await this.fileService.GetDirectory();

        let pathTemplate = (currentPath) => html`
            ${currentPath.split("/").map((subpath) => html`
                / <a href="${subpath}">${subpath}</a>
            `)}
        `;

        console.log(directory);
        console.log(this);
        render(pathTemplate(directory.relativePath), this.shadowRoot.getElementById("wrapper"));
    }
}

customElements.define("demo-path-map", PathMapComponent);