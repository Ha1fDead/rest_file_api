// force imports so web components load. Not a problem w/ webpack where we can automate it
import DirectoryComponent from './directory.comp.js';
import UploadComponent from './upload.comp.js';
import PathMapComponent from './pathmap.comp.js';
import NotFoundComponent from './notfound.comp.js';
import FileService from './file.service.js';
import FileComponent from './file.comp.js';

const template = document.createElement("template");
template.innerHTML = `
<div id="wrapper">
    <demo-path-map path="/"></demo-path-map>
    <demo-directories></demo-directories>
    <demo-upload></demo-upload>
    <demo-not-found></demo-not-found>
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

/**
 * Let's users navigate a server directory files & sub directories
 * 
 * Also allows users to upload, move, copy, delete, and upload files / directories
 */
export default class DirectoryBrowserComponent extends HTMLElement {
    constructor() {
        super();

        const shadow = this.attachShadow({mode: "open"});
        shadow.appendChild(style.cloneNode(true));

        this._wrapper = template.content.cloneNode(true);
        shadow.appendChild(this._wrapper);

        window.addEventListener('popstate', this.UpdateState.bind(this), false);
        window.addEventListener('demo-datachange', this.UpdateState.bind(this), false);
    }

	connectedCallback() {
        this.UpdateState();
    }

    async UpdateState(e) {
        const fs = new FileService();
        const pathmaps = this.shadowRoot.getElementsByTagName("demo-path-map");
    
        for (let path of pathmaps) {
            path.path = fs.GetCurrentRelativePath();
        }
    
        const directorycomp = this.shadowRoot.getElementsByTagName("demo-directories")[0];
        const notfoundcomponent = this.shadowRoot.getElementsByTagName("demo-not-found")[0];

        try {
            const directory = await fs.GetDirectory(fs.GetCurrentRelativePath())
            directorycomp.style.display = "";
            notfoundcomponent.style.display = "none";
            // abuse immutability
            directorycomp.directory = directory;
        }
        catch(e) {
            // 404 or some other error
            // Error message could really be improved here
            notfoundcomponent.style.display = "";
            directorycomp.style.display = "none";
        }
    }
}

customElements.define("demo-directory-browser", DirectoryBrowserComponent);