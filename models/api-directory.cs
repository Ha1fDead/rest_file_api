using System.Collections.Generic;

namespace maplarge_restapicore.models
{
    public class ApiDirectory {
        /// Name of the Directory
        public string Name {get; set;}

        /// The relative path for this Directory that can be sent to the server for operations on its child files / directories
        public string RelativePath {get; set;}

        /// All files directly under this directory
        public IEnumerable<ApiFile> Files {get; set;}

        /// All immediate children of this Directory
        public IEnumerable<string> SubDirectories {get; set;}
    }
}
