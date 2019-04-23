using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace rest_file_api.models.actions
{
    public class ApiMoveFile {
        public string FileName {get; set;}
        public string RelativePathToDirectory {get; set;}
        public string RelativePathToDestDirectory {get; set;}
    }
}