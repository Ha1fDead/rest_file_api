import {html, render} from 'https://unpkg.com/lit-html?module';y


// This uses lit-html (see readme)
const template = document.createElement("template");
template.innerHTML = `<div id="wrapper">
</div>`;

const style = document.createElement("style");
style.textContent = `

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
        let directoriesTemplate = (directory) => html`
            <table>
                <thead>
                    <tr>
                        <th>File Name</th>
                        <th>Size (Bytes)</th>
                        <th>Date Created</th>
                        <th>Date Modified</th>
                    </tr>
                </thead>
                <tbody>
                ${directory.subDirectories.map((dir) => html`
                    <tr><demo-file filename="${dir}" relativepath="${directory.relativePath}"></demo-file></tr>
                `)}
                ${directory.files.map((file) => html`
                    <tr><demo-file filename="${file.name}" relativepath="${directory.relativePath}" sizebytes="${file.sizeBytes}" datemodified="${file.dateModified}" datecreated="${file.dateCreated}"></demo-file></tr>
                `)}
                </tbody>
            </table>
        `;

        render(directoriesTemplate(directory), this.shadowRoot.getElementById("wrapper"));
    }
}

customElements.define("demo-directories", DirectoriesComponent);