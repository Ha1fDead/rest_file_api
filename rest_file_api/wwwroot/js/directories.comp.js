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

    HandleDelete(e, relativePath, fileName, subDirName) {
        if (subDirName) {
            let path = `${relativePath ? `${relativePath}/` : ``}${subDirName}`;
            this.fileService.Delete(path, null).catch((err) => {
                alert(err);
            });
        } else {
            let path = relativePath ? relativePath : ``;
            this.fileService.Delete(path, fileName).then((res) => {
                alert('success');
            }, (err) => {
                alert(err);
            });
        }
    }

    HandleCopy(e, relativePath, fileName, subDirName) {
        if (subDirName) {
            let path = `${relativePath ? `${relativePath}/` : ``}${subDirName}`;
            this.fileService.Copy(path, null, subDirName + '1').catch((err) => {
                alert(err);
            });
        } else {
            let path = relativePath ? relativePath : ``;
            this.fileService.Copy(path, fileName, fileName + '1').then((res) => {
                alert('success');
            }, (err) => {
                alert(err);
            });
        }
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
                        <td><button @click=${e => { this.HandleDelete(e, directory.relativePath, null, dir) }}>Delete</button></td>
                        <td><button @click=${e => { this.HandleCopy(e, directory.relativePath, null, dir)}}>Copy</button></td>
                    </tr>
                `)}
                ${directory.files.map((file) => html`
                    <tr>
                        <td><a href="${this.generateLinkToFile(directory.relativePath, file.name)}">${file.name}</a></td>
                        <td>${file.sizeBytes}</td>
                        <td>${new Date(file.dateCreated).toLocaleDateString()}</td>
                        <td>${new Date(file.dateModified).toLocaleDateString()}</td>
                        <td><button @click=${e => { this.HandleDelete(e, directory.relativePath, file.name, null) }}>Delete</button></td>
                        <td><button @click=${e => { this.HandleCopy(e, directory.relativePath, file.name, null) }}>Copy</button></td>
                    </tr>
                `)}
                </tbody>
            </table>
        `;

        let directory = await this.fileService.GetDirectory(this.fileService.GetCurrentRelativePath());
        render(directoriesTemplate(directory), this.shadowRoot.getElementById("wrapper"));
    }

    generateLinkToDirectory(relativePath, subDirectory) {
        return `/${relativePath ? `${relativePath}/` : ``}${subDirectory}`;
    }

    generateLinkToFile(relativePath, fileName) {
        return `/api/file/download/${relativePath ? `${relativePath}/` : ``}${fileName}`;
    } 
}

customElements.define("demo-directories", DirectoriesComponent);