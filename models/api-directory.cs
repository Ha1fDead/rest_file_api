using System.Collections.Generic;

namespace maplarge_restapicore.models
{
    public class ApiDirectory {
        /// Name of the Directory
        public string Name {get; set;}

        /// All files directly under this directory
        public IEnumerable<ApiFile> Files {get; set;}

        /// All immediate children of this Directory
        public IEnumerable<string> SubDirectories {get; set;}
    }
}
