 export default class FileService {
    constructor() {

    }

    async GetDirectory(relativePathToDirectory) {
        if (relativePathToDirectory == null) {
            relativePathToDirectory = "";
        }

        let res = await fetch(`/api/file?relativePathToDirectory=${relativePathToDirectory}`);
        let data = await res.json();
        return data;
    }

    async UploadFile() {
        
    }

    // optional ?
    async Search() {

    }

    // xtra
    async Copy() {

    }

    // xtra
    async Move() {

    }

    // xtra
    async Delete(relativePathToDirectory, fileName) {
        let res = await fetch(`/api/file?relativePathToDirectory=${relativePathToDirectory}&fileName=${fileName}`, {
            method: 'delete'
        });

        console.log('made delete request: ', res);
    }
}