using System;
using System.Collections.Generic;


namespace maplarge_restapicore.models
{
    public class ApiFile {
        /// Name of the file
        string Name { get; set; }

        /// How large the file is
        /// Pretty sure int is large enough for file sizes
        int SizeBytes {get; set;}

        /// The date this file was created (Server time)
        DateTime DateCreated {get; set;}

        /// The date this file was last modified (Server time)
        DateTime DateModified { get; set;}
    }
}