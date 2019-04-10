using System;
using System.Collections.Generic;


namespace maplarge_restapicore.models
{
    public class ApiFile {
        /// Name of the file
        public string Name { get; set; }

        /// How large the file is
        public long SizeBytes {get; set;}

        /// The date this file was created (Server time)
        public DateTime DateCreated {get; set;}

        /// The date this file was last modified (Server time)
        public DateTime DateModified { get; set;}
    }
}