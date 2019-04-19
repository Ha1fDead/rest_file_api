 export default class FileService {
    constructor() {

    }

    GetCurrentRelativePath() {
        return decodeURI(window.location.pathname.substr(1));
    }

    GenerateLinkToDirectory(relativePath, subDirectory) {
        return `/${relativePath ? `${relativePath}/` : ``}${subDirectory}`;
    }

    GenerateLinkToFile(relativePath, fileName) {
        return `/api/file/download/${relativePath ? `${relativePath}/` : ``}${fileName}`;
    } 

    /**
     * Retrieves a directory & its immediate children
     * @param {string} relativePathToDirectory 
     */
    async GetDirectory(relativePathToDirectory) {
        if (relativePathToDirectory == null) {
            relativePathToDirectory = "";
        }

        let res = await fetch(encodeURI(`/api/file?relativePathToDirectory=${relativePathToDirectory}`))
            .catch(this._HandleNetworkError);

        if (!res.ok) {
            return this._HandleApplicationError(res);
        }
        
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
            body: data
        }).catch(this._HandleNetworkError);

        // operation was successful
        return Promise.resolve();
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
        let res = await fetch(`/api/file/copy?relativePathToDirectory=${relativePathToDirectory}&fileName=${fileName ? fileName : ''}&copyName=${copyName}`, {
            method: 'put'
        }).catch(this._HandleNetworkError);

        if (!res.ok) {
            return this._HandleApplicationError(res);
        }

        // operation was successful
        return Promise.resolve();
    }

    /**
     * Attempts to move a directory or file
     * @param {string} relativePathToDirectory 
     * @param {string} fileName 
     * @param {string} relativePathToDestDirectory 
     */
    async Move(relativePathToDirectory, fileName, relativePathToDestDirectory) {
        let res = await fetch(`/api/file/move?relativePathToDirectory=${relativePathToDirectory}&fileName=${fileName ? fileName : ''}&relativePathToDestDirectory=${relativePathToDestDirectory}`, {
            method: 'put'
        }).catch(this._HandleNetworkError);

        if (!res.ok) {
            return this._HandleApplicationError(res);
        }

        // operation was successful
        return Promise.resolve();
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
        }).catch(this._HandleNetworkError);

        if (!res.ok) {
            return this._HandleApplicationError(res);
        }

        // operation was successful
        return Promise.resolve();
    }

    /**
     * Handles all application errors from server
     * @param {Response} response from the server
     * @returns {Promise<void, string>} a rejected promise with a message to display to users
     */
    async _HandleApplicationError(response) {
        // something wrong with the request
        let body = await response.text();
        if (body.length == 0) {
            // no message from server -- we have nothing useful to tell the user
            return Promise.reject("There was an unexpected problem handling your request. Please try again.");
        }
        
        let errorObject = JSON.parse(body);
        return Promise.reject(errorObject.message);
    }

    /**
     * 
     * @param {Error} networkerror 
     */
    _HandleNetworkError(networkerror) {
        // swallow error. In a "Real" application we would display a notification telling the user that they're offline
        // we would also auto discover reconnect
        return Promise.reject("There was an unexpected problem with your network. Please try again.");
    }
}