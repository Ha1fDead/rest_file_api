using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace maplarge_restapicore.models
{
    public class ApiUploadFile {
        public List<IFormFile> Files {get; set;}
        public string RelativePathToDirectory { get; set; }
    }
}