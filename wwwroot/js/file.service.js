 export default class FileService {
    constructor() {

    }

    GetCurrentRelativePath() {
        return window.location.pathname.substr(1);
    }

    async GetDirectory(relativePathToDirectory) {
        if (relativePathToDirectory == null) {
            relativePathToDirectory = "";
        }

        let res = await fetch(`/api/file?relativePathToDirectory=${relativePathToDirectory}`);
        let data = await res.json();
        return data;
    }

    async UploadFile(filesToUpload, relativePathToDirectory) {
        let data = new FormData();

        data.append('relativePathToDirectory', relativePathToDirectory);
        for (let x = 0; x < filesToUpload.length; x++) {
            data.append('files', filesToUpload.item(x));
        }

        let res = await fetch('/api/file', {
            method: 'post',
            body: data,
            credentials: "include",
            mode: "cors"
        });
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