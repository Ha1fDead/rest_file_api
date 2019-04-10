using System.Collections.Generic;

namespace maplarge_restapicore.models
{
    public class ApiDirectory {
        /// Name of the Directory
        string Name {get; set;}

        /// All files directly under this directory
        IEnumerable<ApiFile> Files {get; set;}

        /// All immediate children of this Directory
        IEnumerable<string> SubDirectories {get; set;}
    }
}
