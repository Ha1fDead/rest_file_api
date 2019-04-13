using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using maplarge_restapicore.models;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace maplarge_restapicore.controllers
{

    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IFileProvider _fileProvider;

        public FileController(
            IConfiguration config,
            IFileProvider fileProvider)
        {
            _config = config;
            _fileProvider = fileProvider;
        }
        
        [HttpGet]
        [Route("")]
        public async Task<ActionResult<ApiDirectory>> Get(string relativePathToDirectory)
        {
            if (string.IsNullOrEmpty(relativePathToDirectory))
            {
                relativePathToDirectory = "";
            }

            var directoryInfo = _fileProvider.GetDirectoryContents(relativePathToDirectory);
            if (!directoryInfo.Exists) 
            {
                return NotFound();
            }

            return FileHelper.GetDirectoryInfo(directoryInfo, relativePathToDirectory);
        }

        [HttpGet]
        [HttpPost]
        [Route("download/{*relativePathToFile}")]
        public async Task<ActionResult> Download(string relativePathToFile)
        {
            if (string.IsNullOrEmpty(relativePathToFile))
            {
                return BadRequest();
            }

            var fileInfo = _fileProvider.GetFileInfo(relativePathToFile);
            if (!fileInfo.Exists) {
                return NotFound();
            }

            var memory = new MemoryStream();  
            using (var stream = fileInfo.CreateReadStream())
            {
                await stream.CopyToAsync(memory);  
            }
            memory.Position = 0;
            return File(memory, "application/octet-stream", fileInfo.Name);
        }

        [HttpPost]
        public async Task<ActionResult<ApiFile>> Upload([FromForm] ApiUploadFile upload)
        {
            if (upload == null || upload.Files == null || upload.Files.Count() == 0) {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(upload.RelativePathToDirectory))
            {
                upload.RelativePathToDirectory = "";
            }

            foreach (var formFile in upload.Files) 
            {
                var fileInfo = _fileProvider.GetFileInfo(Path.Join(upload.RelativePathToDirectory, formFile.FileName));
                if (fileInfo.Exists)
                {
                    return Conflict();
                }
            }

            // https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-2.2
            long size = upload.Files.Sum(f => f.Length);

            var filePath = Path.GetTempFileName();

            foreach (var formFile in upload.Files)
            {
                if (formFile.Length > 0)
                {
                    var fileInfo = _fileProvider.GetFileInfo(Path.Join(upload.RelativePathToDirectory, formFile.FileName));
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                    System.IO.File.Move(filePath, fileInfo.PhysicalPath);
                }
            }

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
            if (string.IsNullOrEmpty(relativePathToDirectory))
            {
                relativePathToDirectory = "";
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                var fullDestPath = this.GetAbsoluteDirectoryPath(relativePathToDirectory);
                if (!ResolvedPathIsValid(fullDestPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return Forbid();
                }
                
                if (!Directory.Exists(fullDestPath))
                {
                    return NotFound();
                }

                Directory.Delete(fullDestPath);
                return Ok();
            }
            else
            {
                var fileInfo = _fileProvider.GetFileInfo(Path.Join(relativePathToDirectory, fileName));
                if (!fileInfo.Exists)
                {
                    return NotFound();
                }

                System.IO.File.Delete(fileInfo.PhysicalPath);
                return Ok();
            }
        }

        [HttpPut]
        [Route("move")]
        // Extra
        public async Task<ActionResult> MoveFile(string relativePathToDirectory, string fileName, string relativePathToDestDirectory)
        {
            var fullOriginalPath = this.GetAbsoluteFilePath(relativePathToDirectory, fileName);
            if (!ResolvedPathIsValid(fullOriginalPath))
            {
                // User may be attempting to view "Up" directories -- app should only let people view "Down"
                return Forbid();
            }

            var fullDestinationPath = this.GetAbsoluteFilePath(relativePathToDestDirectory, fileName);
            if (!ResolvedPathIsValid(fullDestinationPath))
            {
                // User may be attempting to view "Up" directories -- app should only let people view "Down"
                return Forbid();
            }

            if (!System.IO.File.Exists(fullOriginalPath))
            {
                return NotFound();
            }

            if (System.IO.File.Exists(fullDestinationPath))
            {
                return Conflict();
            }

            System.IO.File.Move(fullOriginalPath, fullDestinationPath);
            return Ok();
        }

        [HttpPut]
        [Route("copy")]
        // Extra
        public async Task<ActionResult> CopyFile(string relativePathToDirectory, string fileName, string copyName)
        {
            var fullOriginalPath = this.GetAbsoluteFilePath(relativePathToDirectory, fileName);
            if (!ResolvedPathIsValid(fullOriginalPath))
            {
                // User may be attempting to view "Up" directories -- app should only let people view "Down"
                return Forbid();
            }

            var fullCopyDestination = this.GetAbsoluteFilePath(relativePathToDirectory, copyName);
            if (!ResolvedPathIsValid(fullCopyDestination))
            {
                // User may be attempting to view "Up" directories -- app should only let people view "Down"
                return Forbid();
            }

            if (!System.IO.File.Exists(fullOriginalPath))
            {
                return NotFound();
            }

            if (System.IO.File.Exists(fullCopyDestination))
            {
                return Conflict();
            }

            System.IO.File.Copy(fullOriginalPath, fullCopyDestination);
            return Ok();
        }

        private string GetAbsoluteDirectoryPath(string relativePathToDirectory)
        {
            // empty strings are parsed as nulls -- want them to be treated as empty
            if (string.IsNullOrEmpty(relativePathToDirectory))
            {
                relativePathToDirectory = "";
            }

            return Path.GetFullPath(Path.Join(_config["root_server_directory"], relativePathToDirectory));
        }

        private string GetAbsoluteFilePath(string relativePathToDirectory, string fileName)
        {
            // empty strings are parsed as nulls -- want them to be treated as empty
            if (string.IsNullOrEmpty(relativePathToDirectory))
            {
                relativePathToDirectory = "";
            }

            return Path.GetFullPath(Path.Join(_config["root_server_directory"], relativePathToDirectory, fileName));
        }

        private bool ResolvedPathIsValid(string absolutePath)
        {
            return absolutePath.StartsWith(_config["root_server_directory"]);
        }
    }

    public static class FileHelper
    {
        public static ApiDirectory GetDirectoryInfo(IDirectoryContents directory, string relativePathToDirectory)
        {
            var subdir = new List<string>();
            var files = new List<ApiFile>();
            foreach (var file in directory)
            {
                if (file.IsDirectory)
                {
                    subdir.Add(file.Name);
                }
                else
                {
                    files.Add(new ApiFile()
                    {
                        Name = file.Name,
                        SizeBytes = file.Length,
                        DateModified = file.LastModified.DateTime,
                        DateCreated = File.GetCreationTimeUtc(file.PhysicalPath)
                    });
                }
            }
            var apiDirectory = new ApiDirectory()
            {
                Name = Path.GetFileName(relativePathToDirectory),
                RelativePath = relativePathToDirectory,
                SubDirectories = subdir,
                Files = files
            };
            return apiDirectory;
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