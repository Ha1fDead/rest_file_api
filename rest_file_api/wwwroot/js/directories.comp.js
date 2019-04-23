import {html, render} from 'https://unpkg.com/lit-html?module';


// This uses lit-html (see readme)
const template = document.createElement("template");
template.innerHTML = `<div id="wrapper">
</div>`;

const style = document.createElement("style");
style.textContent = `
    ul {
        display: grid;
        grid-template-rows: 1fr;
        grid-template-columns: 1fr;
        list-style: none;
        padding: 0px;
    }

    ul > li {
        margin: 10px;
    }
`;

export default class DirectoriesComponent extends HTMLElement {
    constructor() {
        super();

        const shadow = this.attachShadow({mode: "open"});
        shadow.appendChild(style.cloneNode(true));

        this._wrapper = template.content.cloneNode(true);
        shadow.appendChild(this._wrapper);
    }

    static get observedAttributes() {
        return ["directory"];
    }

    /**
     * The Directory that this component is going to display
     * 
     * @param {object} value
     */
    set directory(value) {
        this._directory = value;
        this._update(value);
    }

    /**
     * The Directory that this component is going to display
     */
    get directory() {
        return this._directory;
    }

	connectedCallback() {
		this.UpgradeProperties();
    }

    UpgradeProperties() {
        for (const prop of DirectoriesComponent.observedAttributes) {
            this.UpgradeProperty(prop);
        }
    }

    UpgradeProperty(prop) {
        if (this.hasOwnProperty(prop)) {
            const value = this[prop];
            delete this[prop];
            this[prop] = value;
        }
    }

    async _update(directory) {
        // sort could be vastly improved -- case insensitive, character behavior, etc.
        // additional considerations for system (Linux always does Directories then Files, windows mix/matches IIRC)
        // Since linux approach is easiest, I just went with that.
        directory.subDirectories.sort((a,b) => a > b);
        directory.files.sort((a, b) => a.fileName > b.fileName);
        let directoriesTemplate = (directory) => html`
            <ul>
                ${directory.subDirectories.map((dir) => html`
                    <li><demo-file filename="${dir}" relativepath="${directory.relativePath}"></demo-file></li>
                `)}
                ${directory.files.map((file) => html`
                    <li><demo-file filename="${file.name}" relativepath="${directory.relativePath}" sizebytes="${file.sizeBytes}" datemodified="${file.dateModified}" datecreated="${file.dateCreated}"></demo-file></li>
                `)}
            </ul>
        `;

        render(directoriesTemplate(directory), this.shadowRoot.getElementById("wrapper"));
    }
}

customElements.define("demo-directories", DirectoriesComponent);