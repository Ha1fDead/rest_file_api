using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplarge_restapicore.models;
using System.Linq;

namespace maplarge_restapicore.controllers
{

    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        public readonly string server_path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        [HttpGet]
        [Route("")]
        public async Task<ActionResult<ApiDirectory>> Get(string relativePathToDirectory)
        {
            if (string.IsNullOrEmpty(relativePathToDirectory) || relativePathToDirectory == "/") {
                relativePathToDirectory = "";
            }

            var resolvedPath = Path.GetFullPath(Path.Combine(server_path, relativePathToDirectory));
            if (!resolvedPath.StartsWith(server_path)) {
                // User may be attempting to view "Up" directories -- app should only let people view "Down"
                return StatusCode(403);
            }

            if (!Directory.Exists(resolvedPath)) 
            {
                return StatusCode(404);
            }

            return FileHelper.GetDirectoryInfo(relativePathToDirectory, resolvedPath);
        }

        [HttpGet]
        [HttpPost]
        [Route("download/{*relativePathToFile}")]
        public async Task<ActionResult> Download(string relativePathToFile)
        {
            var resolvedPath = Path.GetFullPath(Path.Combine(server_path, relativePathToFile));
            if (!resolvedPath.StartsWith(server_path)) {
                // User may be attempting to view "Up" directories -- app should only let people view "Down"
                return StatusCode(403);
            }

            if (!System.IO.File.Exists(resolvedPath)) {
                return StatusCode(404);
            }

            var memory = new MemoryStream();  
            using (var stream = new FileStream(resolvedPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);  
            }
            memory.Position = 0;
            return File(memory, "application/octet-stream", Path.GetFileName(resolvedPath));
        }

        [HttpPost]
        public async Task<ActionResult<ApiFile>> Upload([FromForm] ApiUploadFile upload)
        {
            if (upload == null || string.IsNullOrEmpty(upload.RelativePathToDirectory) || upload.Files == null || upload.Files.Count() == 0) {
                return BadRequest();
            }

            var resolvedPath = Path.GetFullPath(Path.Combine(server_path, upload.RelativePathToDirectory));
            if (!resolvedPath.StartsWith(server_path)) {
                // User may be attempting to view "Up" directories -- app should only let people view "Down"
                return StatusCode(403);
            }

            // https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-2.2
            // ensure multiple people can upload file simultaneously
            long size = upload.Files.Sum(f => f.Length);

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            foreach (var formFile in upload.Files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = upload.Files.Count, size, filePath});
        }

        [HttpGet]
        [Route("search/{filename}")]
        // Extra maybe?
        public async Task<ActionResult> Search(string filename)
        {
            return StatusCode(StatusCodes.Status405MethodNotAllowed);
        }

        [HttpDelete]
        [Route("")]
        // Extra
        public async Task<ActionResult> Delete(string relativePathToDirectory, string fileName)
        {
            return StatusCode(StatusCodes.Status405MethodNotAllowed);
        }

        [HttpPut]
        [Route("move")]
        // Extra
        public async Task<ActionResult> MoveFile(string file_to_move, string relativePathToDirectory)
        {
            return StatusCode(StatusCodes.Status405MethodNotAllowed);
        }

        [HttpPut]
        [Route("copy")]
        // Extra
        public async Task<ActionResult> CopyFile(string relativePathToDirectory, string file_to_copy, string name_of_copy)
        {
            return StatusCode(StatusCodes.Status405MethodNotAllowed);
        }
    }

    public static class FileHelper
    {
        public static ApiDirectory GetDirectoryInfo(string relativePathToDirectory, string resolvedPath)
        {
            return GetDirectoryInfo(relativePathToDirectory, new DirectoryInfo(resolvedPath));
        }

        public static ApiDirectory GetDirectoryInfo(string relativePathToDirectory, DirectoryInfo info)
        {
            var allfiles = new List<ApiFile>();
            foreach(var file in info.GetFiles())
            {
                allfiles.Add(GetFileInfo(file));
            }

            Console.WriteLine(allfiles.Count);

            var allDirectories = new List<string>();
            foreach(var dir in info.GetDirectories())
            {
                allDirectories.Add(dir.Name);
            }

            var directory = new ApiDirectory
            {
                Name = info.Name,
                RelativePath = relativePathToDirectory,
                Files = allfiles,
                SubDirectories = allDirectories
            };

            return directory;
        }

        public static ApiFile GetFileInfo(FileInfo info)
        {
            var file = new ApiFile
            {
                Name = info.Name,
                DateCreated = info.CreationTimeUtc,
                DateModified = info.LastWriteTimeUtc,
                SizeBytes = info.Length
            };

            return file;
        }

        // Oof directory walking can get complex. Mapping my home directory takes FOR-EH-VER
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree
        public static long DirSizeRecursive(DirectoryInfo d) 
        {    
            long size = 0;    
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis) 
            {      
                size += fi.Length;    
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis) 
            {
                size += DirSizeRecursive(di);   
            }
            return size;  
        }
    }
}