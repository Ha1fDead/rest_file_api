let template = document.createElement("template");
template.innerHTML = `
    <div>
        <label for="upload">Upload a File</label>
        <input type="file" id="upload">
    </div>
`;


export default class UploadComponent extends HTMLElement {
    constructor() {
        super();

        const shadow = this.attachShadow({ mode: "open"});
        shadow.appendChild(template.content.cloneNode(true));
    }
}

customElements.define("demo-upload", UploadComponent);
