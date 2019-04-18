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
        window.addEventListener('popstate', event => {
            // Known limitation -- if user manually enters a URL we cannot intercept that
            // Apparently not even Angular does it, which is fascinating!
            this._update();
        }, false);
    }

    async _update() {
        let directory = await this.fileService.GetDirectory(this.fileService.GetCurrentRelativePath());

        // need to split the string but return the entire string from the split
        let pathTemplate = (currentPath) => html`
            ${this.getPaths(currentPath).map((subpath) => html`
                / <a @click=${e => this.HandleNavDirectory(e, subpath.previousPath, subpath.part)} href="${subpath.previousPath}">${subpath.part}</a>
            `)}
        `;

        render(pathTemplate(directory.relativePath), this.shadowRoot.getElementById("wrapper"));
    }

    /**
     * 
     * @param {Event} e 
     * @param {string} relativeDirectory 
     * @param {string} directoryName 
     */
    HandleNavDirectory(e, relativeDirectory, directoryName) {
        e.preventDefault();
        history.pushState({}, directoryName, relativeDirectory);
        this._update();
        return false;
    }

    getPaths(path) {
        let split = this.splitStringIntoParts(path, "/");
        split.splice(0, 0, {previousPath: "/", part: "home"});
        return split;
    }

    /**
     * 
     * @param {string} str 
     * @param {string} delimiter 
     * @returns {object[]}
     */
    splitStringIntoParts(str, delimiter) {
        if (str == null || str.length === 0) {
            return [];
        }

        let split = str.split(delimiter);
        let parts = [];
        for (let x = 0; x < split.length; x++) {
            let previous = `/${split.slice(0, x + 1).join("/")}`;
            parts[x] = {
                previousPath: previous,
                part: split[x]
            };
        }

        return parts;
    }
}

customElements.define("demo-path-map", PathMapComponent);