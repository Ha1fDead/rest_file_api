import FileService from "./file.service.js";

let template = document.createElement("template");
template.innerHTML = `
    <form id="uploadform">
        <fieldset id="uploaddrop">
            <label for="upload">Upload a File</label>
            <input type="file" id="file" multiple>
        </fieldset>
        <button type="submit">Upload File</button>
    </form>
`;


export default class UploadComponent extends HTMLElement {
    constructor() {
        super();

        const shadow = this.attachShadow({ mode: "open"});
        shadow.appendChild(template.content.cloneNode(true));

        let dragdrop = shadow.getElementById("uploaddrop");
        dragdrop.addEventListener("dragennter", this.handleDragEnter.bind(this), false);
        dragdrop.addEventListener("dragover", this.handleDragOver.bind(this), false);
        dragdrop.addEventListener("drop", this.handleDrop.bind(this), false);

        let uploadform = shadow.getElementById("uploadform");
        uploadform.addEventListener("submit", this.handleUserUpload.bind(this), false);
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

        let fileinput = this.shadowRoot.getElementById("file");

        if (fileinput.files.length == 0) {
            alert("You must specify a file to be uploaded");
        }

        let fileService = new FileService();
        fileService.UploadFile(fileinput.files, fileService.GetCurrentRelativePath());

        return false;
    }
}

customElements.define("demo-upload", UploadComponent);
