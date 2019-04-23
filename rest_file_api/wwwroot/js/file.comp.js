import {html, render} from 'https://unpkg.com/lit-html?module';
import FileService from './file.service.js';

const template = document.createElement("template");
template.innerHTML = `<div id="wrapper">
</div>`;

const style = document.createElement("style");
style.textContent = `

`;

export default class FileComponent extends HTMLElement {
    constructor() {
        super();

        this.fileService = new FileService();

        const shadow = this.attachShadow({mode: "open"});
        shadow.appendChild(style.cloneNode(true));

        this._wrapper = template.content.cloneNode(true);
        shadow.appendChild(this._wrapper);
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
        let path, file, subdir;
        if (subDirName) {
            path = `${relativePath ? `${relativePath}/` : ``}${subDirName}`;
            file = null;
            let dirInput = this.shadowRoot.getElementById("dircopyinput");
            subdir = dirInput.value;
        } else {
            path = relativePath ? relativePath : ``;
            file = fileName;
            let fileInput = this.shadowRoot.getElementById("filecopyinput");
            subdir = fileInput.value;
        }

        this.fileService.Copy(path, file, subdir).then((res) => {
            alert('success');
        }, (err) => {
            alert(err);
        });
    }

    HandleMove(e, relativePath, fileName, subDirName) {
        let path, file, destPath;
        if (subDirName) {
            path = `${relativePath ? `${relativePath}/` : ``}${subDirName}`;
            file = null;
            let dirInput = this.shadowRoot.getElementById("dirmoveinput");
            destPath = dirInput.value;
        } else {
            path = relativePath ? relativePath : ``;
            file = fileName;
            let fileInput = this.shadowRoot.getElementById("filemoveinput");
            destPath = fileInput.value;
        }

        this.fileService.Move(path, file, destPath).then((res) => {
            alert('success');
        }, (err) => {
            alert(err);
        });
    }

    static get observedAttributes() {
        return ["filename", "relativepath", "sizebytes", "datecreated", "datemodified"];
    }

    /**
     * The name of the file
     * 
     * @param {string} value
     */
    set filename(value) {
        this.setAttribute("filename", value);
    }

    /**
     * The name of the file
     */
    get filename() {
        return this.getAttribute("filename");
    }

    /**
     * The relative path to this file
     * 
     * @param {string} valueReferenceError: FileService is not defined[Learn More] file.comp.js:14:9
    FileComponent https://localhost:5001/js/file.comp.js
     */
    set relativepath(value) {
        this.setAttribute("relativepath", value);
    }

    get relativepath() {
        return this.getAttribute("relativepath");
    }

    set sizebytes(value) {
        this.setAttribute("sizebytes", value);
    }

    get sizebytes() {
        return this.getAttribute("sizebytes");
    }

    set datecreated(value) {
        this.setAttribute("datecreated", value);
    }

    get datecreated() {
        return this.getAttribute("datecreated");
    }

    set datemodified(value) {
        this.setAttribute("datemodified", value);
    }

    get datemodified() {
        return this.getAttribute("datemodified");
    }

    attributeChangedCallback(name, oldValue, newValue) {
        this._update();
    }

	connectedCallback() {
		this.UpgradeProperties();
    }

    UpgradeProperties() {
        for (const prop of FileComponent.observedAttributes) {
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

    /**
     * 
     * @param {Event} e 
     * @param {string} relativeDirectory 
     * @param {string} directoryName 
     */
    HandleNavDirectory(e, relativeDirectory, directoryName) {
        e.preventDefault();
        history.pushState({}, directoryName, encodeURI(relativeDirectory));
        var popStateEvent = new PopStateEvent('popstate', { state: {} });
        dispatchEvent(popStateEvent);
        return false;
        // an alternative approach would be to use webworkers that can intercept all http requests
        // find anything not going to a /api route (but still going to this domain) and prevent it from firing
        // but this works and is easier to implement
    }

    _update() {
        let directoryTemplate = (dirName, relativePath) => html`
            <div>
                <a 
                    @click=${e => { this.HandleNavDirectory(e, this.fileService.GenerateLinkToDirectory(relativePath, dirName), dirName)}}
                    href="${encodeURI(this.fileService.GenerateLinkToDirectory(relativePath, dirName))}">
                    ${dirName}
                </a>
            </div>
            <button @click=${e => { this.HandleDelete(e, relativePath, null, dirName) }}>Delete</button>
            <button @click=${e => { this.HandleCopy(e, relativePath, null, dirName) }}>Copy</button>
            <input id="dircopyinput" type="text" value="${relativePath}/${dirName}1"/>
            <button @click=${e => { this.HandleMove(e, relativePath, null, dirName) }}>Move</button>
            <input id="dirmoveinput" type="text" value="${relativePath}/${dirName}"/>
        `;
        let fileTemplate = (fileName, relativePath, sizebytes, dateCreated, dateModified) => html`
            <div>
                <a 
                    href="${encodeURI(this.fileService.GenerateLinkToFile(relativePath, fileName))}">
                    ${fileName}
                </a>
            </div>
            <div>${relativePath}</div>
            <div>${sizebytes}</div>
            <div>${new Date(dateCreated).toLocaleDateString()}</div>
            <div>${new Date(dateModified).toLocaleDateString()}</div>
            <button @click=${e => { this.HandleDelete(e, relativePath, fileName, null) }}>Delete</button>
            <button @click=${e => { this.HandleCopy(e, relativePath, fileName, null) }}>Copy</button>
            <input id="filecopyinput" type="text" value="${fileName}"/>
            <button @click=${e => { this.HandleMove(e, relativePath, fileName, null) }}>Move</button>
            <input id="filemoveinput" type="text" value="${relativePath}/${fileName}"/>
        `;

        if (this.sizebytes == null) {
            // I could move this into a separate "Directory" component, but the behavior is similar enough we can cheese it
            render(directoryTemplate(this.filename, this.relativepath), this.shadowRoot.getElementById("wrapper"));
        } else {
            render(fileTemplate(this.filename, this.relativepath, this.sizebytes, this.datecreated, this.datemodified), this.shadowRoot.getElementById("wrapper"));
        }
    }
}

customElements.define("demo-file", FileComponent);