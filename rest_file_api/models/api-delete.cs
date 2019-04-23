using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace rest_file_api.models
{
    public class ApiDeleteFile {
        public string RelativePathToDirectory {get; set;}
        public string FileName {get; set;}
    }
}