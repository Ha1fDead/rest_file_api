 export default class FileService {
    constructor() {

    }

    GetCurrentRelativePath() {
        return window.location.pathname.substr(1);
    }

    /**
     * Retrieves a directory & its immediate children
     * @param {string} relativePathToDirectory 
     */
    async GetDirectory(relativePathToDirectory) {
        if (relativePathToDirectory == null) {
            relativePathToDirectory = "";
        }

        let res = await fetch(`/api/file?relativePathToDirectory=${relativePathToDirectory}`);
        let data = await res.json();
        return data;
    }

    /**
     * Attempts to upload files
     * @param {FileList} filesToUpload 
     * @param {string} relativePathToDirectory 
     */
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

    /**
     * Attempts to copy a directory or file
     * @param {string} relativePathToDirectory 
     * @param {string} fileName 
     * @param {string} copyName 
     */
    async Copy(relativePathToDirectory, fileName, copyName) {
        let res = await fetch(`/api/file/copy?relativePathToDirectory=${relativePathToDirectory}&fileName=${fileName}&copyName=${copyName}`, {
            method: 'put'
        });

        console.log('made move request', res);
    }

    /**
     * Attempts to move a directory or file
     * @param {string} relativePathToDirectory 
     * @param {string} fileName 
     * @param {string} relativePathToDestDirectory 
     */
    async Move(relativePathToDirectory, fileName, relativePathToDestDirectory) {
        let res = await fetch(`/api/file/move?relativePathToDirectory=${relativePathToDirectory}&fileName=${fileName}&relativePathToDestDirectory=${relativePathToDestDirectory}`, {
            method: 'put'
        });

        console.log('made move request', res);
    }

    /**
     * Attempts to delete a directory or file
     * @param {string} relativePathToDirectory 
     * @param {string} fileName 
     * @returns {Promise<void, string>}
     */
    async Delete(relativePathToDirectory, fileName) {
        let res = await fetch(`/api/file?relativePathToDirectory=${relativePathToDirectory}&fileName=${fileName ? fileName : ''}`, {
            method: 'delete'
        }).catch((networkerror) => {
            // swallow error
            return Promise.reject("There was an unexpected problem with your network. Please try again.");
        });

        if (!res.ok) {
            // something wrong with the request
            let body = await res.text();
            if (body.length == 0) {
                // no message from server -- we have nothing useful to tell the user
                return Promise.reject("There was an unexpected problem handling your request. Please try again.");
            }
            console.log(body);
            let obj = JSON.parse(body);
            return Promise.reject(obj.message);
        }

        // operation was successful
        return Promise.resolve();
    }
}