using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplarge_restapicore.models;

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
        [Route("download/{relativePathToFile}")]
        public async Task<ActionResult> Download(string relativePathToFile)
        {
            // ensure multiple people can download file simultaneously
            // https://stackoverflow.com/questions/42460198/return-file-in-asp-net-core-web-api
            return StatusCode(StatusCodes.Status405MethodNotAllowed);
        }

        [HttpPost]
        public async Task<ActionResult<ApiFile>> Upload(string filename)
        {
            // ensure multiple people can upload file simultaneously
            return StatusCode(StatusCodes.Status405MethodNotAllowed);
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