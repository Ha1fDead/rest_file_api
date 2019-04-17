import {html, render} from 'https://unpkg.com/lit-html?module';
import FileService from "./file.service.js";

let template = document.createElement("template");
template.innerHTML = `
`;


export default class UploadComponent extends HTMLElement {
    constructor() {
        super();

        this.uploadMessage = null;
        this.uploadSuccessful = null;

        const shadow = this.attachShadow({ mode: "open"});
        shadow.appendChild(template.content.cloneNode(true));
        this._update();
    }

    handleDragEnter(event) {
        event.stopPropagation();
        event.preventDefault();
    }

    handleDragOver(event) {
        event.stopPropagation();
        event.preventDefault();
    }

    handleDrop(event) {
        event.stopPropagation();
        event.preventDefault();

        const files = event.dataTransfer.files;
        let filecontrol = this.shadowRoot.getElementById("file");
        filecontrol.files = files;
    }

    handleUserUpload(event) {
        event.preventDefault();

        this.uploadSuccessful = null;
        this.uploadMessage = null;

        let fileinput = this.shadowRoot.getElementById("file");

        if (fileinput.files.length == 0) {
            alert("You must specify a file to be uploaded");
        }

        let fileService = new FileService();
        fileService.UploadFile(fileinput.files, fileService.GetCurrentRelativePath())
            .then(() => {
                this.uploadSuccessful = true;
                this.uploadMessage = "Your files have been uploaded successfully";
                fileinput.value = null; // https://stackoverflow.com/a/24608023/2383477
                this._update();
            }, (err) => {
                this.uploadSuccessful = false;
                this.uploadMessage = err;
                this._update();
            });

        this._update(); // clear form fields
        return false;
    }

    _update() {
        let formTemplate = (uploadSuccessful, message) => html`
            <form id="uploadform" @submit=${ e => {this.handleUserUpload(e) }}>
                <fieldset id="uploaddrop" @drop=${e => { this.handleDrop(e); }} @dragenter=${e => { this.handleDragEnter(e) }} @dragover=${e => { this.handleDragOver(e) }}>
                    <label for="upload">Upload a File</label>
                    <input type="file" id="file" multiple>
                </fieldset>
                ${
                    uploadSuccessful !== null ? html`<p style="${uploadSuccessful ? 'border: 1px solid green;' : 'border: 1px solid red;'}">${ message }</p>` : ''
                }
                <button type="submit">Upload File</button>
            </form>
        `;

        render(formTemplate(this.uploadSuccessful, this.uploadMessage), this.shadowRoot.getRootNode());
    }
}

customElements.define("demo-upload", UploadComponent);
