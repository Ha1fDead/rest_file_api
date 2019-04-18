import DirectoriesComponent from './directories.comp.js';
import UploadComponent from './upload.comp.js';
import PathMapComponent from './pathmap.comp.js';
import NotFoundComponent from './notfound.comp.js';
import FileService from './file.service.js';


window.addEventListener('popstate', event => {
    let fs = new FileService();
    let pathmaps = document.getElementsByTagName("demo-path-map");

    for (const path of pathmaps) {
        path.path = fs.GetCurrentRelativePath();
    }

}, false);