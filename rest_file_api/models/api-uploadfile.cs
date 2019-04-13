using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace rest_file_api.models
{
    public class ApiUploadFile {
        public List<IFormFile> Files {get; set;}
        public string RelativePathToDirectory { get; set; }
    }
}