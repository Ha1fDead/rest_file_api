import {html, render} from 'https://unpkg.com/lit-html?module';
import FileService from './file.service.js';


// This uses lit-html (see readme)
const template = document.createElement("template");
template.innerHTML = `<div>
    <h1>404 -- Oops</h1>
    <p>You can <a href="/">go home</a> to get back to what you were doing</p>
</div>`;

const style = document.createElement("style");
style.textContent = `

`;

export default class NotFoundComponent extends HTMLElement {


    constructor() {
        super();

        const shadow = this.attachShadow({mode: "open"});
        shadow.appendChild(style.cloneNode(true));

        this._wrapper = template.content.cloneNode(true);
        shadow.appendChild(this._wrapper);
    }
}

customElements.define("demo-not-found", NotFoundComponent);