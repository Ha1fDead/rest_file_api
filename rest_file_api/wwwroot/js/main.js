import DirectoriesComponent from './directories.comp.js';
import UploadComponent from './upload.comp.js';
import PathMapComponent from './pathmap.comp.js';
import NotFoundComponent from './notfound.comp.js';
import FileService from './file.service.js';
import FileComponent from './file.comp.js';



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
    
        let directorycomp = document.getElementsByTagName("demo-directories")[0];
        let notfoundcomponent = document.getElementsByTagName("demo-not-found")[0];

        try {
            let directory = await fs.GetDirectory(fs.GetCurrentRelativePath())
            directorycomp.style.display = "";
            notfoundcomponent.style.display = "none";
            directorycomp.directory = directory;
        }
        catch(e) {
            // 404 or some other error
            // Error message could really be improved here
            notfoundcomponent.style.display = "";
            directorycomp.style.display = "none";
        }
    }
}

const app = new Main();
// popstate does not always get called from all browsers on init
app.UpdateState();