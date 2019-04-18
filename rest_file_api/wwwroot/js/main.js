import DirectoriesComponent from './directories.comp.js';
import UploadComponent from './upload.comp.js';
import PathMapComponent from './pathmap.comp.js';
import NotFoundComponent from './notfound.comp.js';
import FileService from './file.service.js';



class Main {
    constructor() {

        window.addEventListener('popstate', this.UpdateState.bind(this), false);
    }

    async UpdateState(e) {
        let fs = new FileService();
        let pathmaps = document.getElementsByTagName("demo-path-map");
    
        for (const path of pathmaps) {
            path.path = fs.GetCurrentRelativePath();
        }
    
        let directory = await fs.GetDirectory(fs.GetCurrentRelativePath());
        let directorycomp = document.getElementsByTagName("demo-directories");
        for (const dir of directorycomp) {
            dir.directory = directory;
        }
    }
}

const app = new Main();
// popstate does not always get called from all browsers
app.UpdateState();