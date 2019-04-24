/**
 * Makes Restful calls to the server for File actions
 */
export default class FileService {
    constructor() {

    }

    /**
     * Gets the current relative path from the URL
     */
    GetCurrentRelativePath() {
        return decodeURI(window.location.pathname.substr(1));
    }

    /**
     * Generates the (NOT ESCAPED) link to a sub-directory
     * @param {string} relativePath to the subdirectory
     * @param {string} subDirectory to generate the link for
     */
    GenerateLinkToDirectory(relativePath, subDirectory) {
        return `/${relativePath ? `${relativePath}/` : ``}${subDirectory}`;
    }

    /**
     * Generates the (NOT ESCAPED) link to download a file
     * @param {string} relativePath to the file
     * @param {string} fileName to generate the link for
     */
    GenerateLinkToFile(relativePath, fileName) {
        return `/api/file/download/${relativePath ? `${relativePath}/` : ``}${fileName}`;
    } 

    /**
     * Retrieves a directory & its immediate children
     * @param {string} relativePathToDirectory to be retrieved
     */
    async GetDirectory(relativePathToDirectory) {
        if (relativePathToDirectory == null) {
            relativePathToDirectory = "";
        }

        const res = await fetch(encodeURI(`/api/file?relativePathToDirectory=${relativePathToDirectory}`))
            .catch(this._HandleNetworkError);

        if (!res.ok) {
            return this._HandleApplicationError(res);
        }
        
        const data = await res.json();
        return data;
    }

    /**
     * Attempts to upload files
     * @param {FileList} filesToUpload 
     * @param {string} relativePathToDirectory where the files will be uploaded to
     */
    async UploadFile(filesToUpload, relativePathToDirectory) {
        const data = new FormData();

        data.append('relativePathToDirectory', relativePathToDirectory);
        for (let x = 0; x < filesToUpload.length; x++) {
            data.append('files', filesToUpload.item(x));
        }

        const res = await fetch('/api/file', {
            method: 'post',
            body: data
        }).catch(this._HandleNetworkError);

        if (!res.ok) {
            return this._HandleApplicationError(res);
        }

        // operation was successful
        window.dispatchEvent(new Event('demo-datachange'));
        return Promise.resolve();
    }

    // optional ?
    async Search() {

    }

    /**
     * Attempts to copy a directory or file
     * @param {string} relativePathToDirectory to the file or directory to be copied
     * @param {string | null} fileName of the file to be copied
     * @param {string} copyName the name of the to-be copied file or directory
     */
    async Copy(relativePathToDirectory, fileName, copyName) {
        const data = {
            RelativePathToDirectory: relativePathToDirectory,
            FileName: fileName ? fileName : '',
            CopyName: copyName
        };
        const res = await fetch(`/api/file/copy`, {
            method: 'put',
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data)
        }).catch(this._HandleNetworkError);

        if (!res.ok) {
            return this._HandleApplicationError(res);
        }

        // operation was successful
        window.dispatchEvent(new Event('demo-datachange'));
        return Promise.resolve();
    }

    /**
     * Attempts to move a directory or file
     * @param {string} relativePathToDirectory to the file to be moved
     * @param {string} fileName of the file to be moved
     * @param {string} relativePathToDestinationFile the relative destination path w/ the file
     * 
     * Move is special, because existing `mv` functions on Unix behave in a certain way, users would expect move to also handle renames instead of just directory moving
     * So the `relativePathToDestinationFile` should be the full-relative-path i.e. `/path/to/my/file.txt` so users could rename `foo.txt` to `file.txt`
     */
    async Move(relativePathToDirectory, fileName, relativePathToDestinationFile) {
        const data = {
            FileName: fileName ? fileName : '',
            RelativePathToDirectory: relativePathToDirectory,
            RelativePathToDestDirectory: relativePathToDestinationFile
        };
        const res = await fetch(`/api/file/move`, {
            method: 'put',
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data)
        }).catch(this._HandleNetworkError);

        if (!res.ok) {
            return this._HandleApplicationError(res);
        }

        // operation was successful
        window.dispatchEvent(new Event('demo-datachange'));
        return Promise.resolve();
    }

    /**
     * Attempts to delete a directory or file
     * @param {string} relativePathToDirectory of the file or directory to be deleted
     * @param {string | null} fileName (optional) - the filename to be deleted, if deleting a file
     * @returns {Promise<void, string>}
     */
    async Delete(relativePathToDirectory, fileName) {
        const data = {
            FileName: fileName ? fileName : '',
            RelativePathToDirectory: relativePathToDirectory
        };
        const res = await fetch(`/api/file`, {
            method: 'delete',
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data)
        }).catch(this._HandleNetworkError);

        if (!res.ok) {
            return this._HandleApplicationError(res);
        }

        // operation was successful
        window.dispatchEvent(new Event('demo-datachange'));
        return Promise.resolve();
    }

    /**
     * Handles all application errors from server
     * @param {Response} response from the server
     * @returns {Promise<void, string>} a rejected promise with a message to display to users
     */
    async _HandleApplicationError(response) {
        // something wrong with the request
        const body = await response.text();
        if (body.length == 0) {
            // no message from server -- we have nothing useful to tell the user
            return Promise.reject("There was an unexpected problem handling your request. Please try again.");
        }
        
        const errorObject = JSON.parse(body);
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