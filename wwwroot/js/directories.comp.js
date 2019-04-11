import {html, render} from 'https://unpkg.com/lit-html?module';
import FileService from './file.service.js';


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
                    <tr>
                        <td><a href="${this.generateLinkToDirectory(directory.relativePath, dir)}">${dir}</a></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td><button @click=${e => { this.HandleDelete(e, directory.relativePath, null) }}>Delete</button></td>
                    </tr>
                `)}
                ${directory.files.map((file) => html`
                    <tr>
                        <td><a href="${this.generateLinkToFile(directory.relativePath, file.name)}">${file.name}</a></td>
                        <td>${file.sizeBytes}</td>
                        <td>${new Date(file.dateCreated).toLocaleDateString()}</td>
                        <td>${new Date(file.dateModified).toLocaleDateString()}</td>
                        <td><button @click=${e => { this.HandleDelete(e, directory.relativePath, file.name) }}>Delete</button></td>
                    </tr>
                `)}
                </tbody>
            </table>
        `;

        // Todo -- move this to invert it
        var cwd = window.location.pathname.substr(1);
        let directory = await this.fileService.GetDirectory(cwd);
        console.log(directory);
        console.log(this);
        render(directoriesTemplate(directory), this.shadowRoot.getElementById("wrapper"));
        console.log('rendered');
    }

    generateLinkToDirectory(relativePath, subDirectory) {
        return `/${relativePath ? `${relativePath}/` : ``}${subDirectory}`;
    }

    generateLinkToFile(relativePath, fileName) {
        // todo -- friendify filename and relative path
        return `/api/file/download/${relativePath}/${fileName}`;
    } 
}

customElements.define("demo-directories", DirectoriesComponent);