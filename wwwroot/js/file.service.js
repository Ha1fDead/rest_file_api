 export default class FileService {
    constructor() {

    }

    async GetFiles() {
        let res = await fetch('api/file');
        let data = await res.json();
        console.log(data);
    }
}