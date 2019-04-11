 export default class FileService {
    constructor() {

    }

    async GetDirectory() {
        let res = await fetch('/api/file');
        let data = await res.json();
        return data;
    }
}