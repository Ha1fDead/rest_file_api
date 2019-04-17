using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rest_file_api.models;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace rest_file_api.controllers
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
        public ActionResult<ApiDirectory> Get(string relativePathToDirectory)
        {
            if (string.IsNullOrEmpty(relativePathToDirectory))
            {
                relativePathToDirectory = "";
            }

            var directoryInfo = _fileProvider.GetDirectoryContents(relativePathToDirectory);
            if (!directoryInfo.Exists) 
            {
                return NotFound(new ApiError("The directory requested does not exist"));
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
                return BadRequest(new ApiError("You must specify a valid path to the file"));
            }

            var fileInfo = _fileProvider.GetFileInfo(relativePathToFile);
            if (!fileInfo.Exists)
            {
                return NotFound(new ApiError("The file you requested to download does not exist or the path is invalid"));
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
                return BadRequest(new ApiError("You must specify a file to be uploaded"));
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
                    // In "real" production circumstances, I would figure out if we can just replace the file or increment its filename
                    return Conflict(new ApiError("You are attempted to upload a file that already exists. Delete the file and try again"));
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
        public ActionResult Search(string filename)
        {
            return StatusCode(StatusCodes.Status405MethodNotAllowed);
        }

        [HttpDelete]
        [Route("")]
        // Extra
        public ActionResult Delete(string relativePathToDirectory, string fileName)
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
                    return StatusCode(403);
                }
                
                if (!Directory.Exists(fullDestPath))
                {
                    return NotFound(new ApiError("The file you are requesting to delete does not exist"));
                }

                Directory.Delete(fullDestPath, true);
                return Ok();
            }
            else
            {
                var fileInfo = _fileProvider.GetFileInfo(Path.Join(relativePathToDirectory, fileName));
                if (!fileInfo.Exists)
                {
                    return NotFound(new ApiError("The directory you are requesting to delete does not exist"));
                }

                System.IO.File.Delete(fileInfo.PhysicalPath);
                return Ok();
            }
        }

        [HttpPut]
        [Route("move")]
        // Extra
        public ActionResult Move(string relativePathToDirectory, string fileName, string relativePathToDestDirectory)
        {
            if (string.IsNullOrEmpty(relativePathToDirectory))
            {
                relativePathToDirectory = "";
            }

            if (string.IsNullOrEmpty(relativePathToDestDirectory))
            {
                relativePathToDestDirectory = "";
            }

            if (string.IsNullOrEmpty(fileName))
            {
                // directory move
                var fullOriginalPath = this.GetAbsoluteDirectoryPath(relativePathToDirectory);
                if (!ResolvedPathIsValid(fullOriginalPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return StatusCode(403);
                }

                var fullDestinationPath = this.GetAbsoluteDirectoryPath(relativePathToDestDirectory);
                if (!ResolvedPathIsValid(fullDestinationPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return StatusCode(403);
                }

                if (!System.IO.Directory.Exists(fullOriginalPath))
                {
                    return NotFound(new ApiError("The file you are requesting to move does not exist"));
                }

                if (System.IO.Directory.Exists(fullDestinationPath))
                {
                    // In "real" production circumstances, I would figure out if we can just replace the file or increment its filename
                    return Conflict(new ApiError("There is already a file in the destination directory with that name. Delete that file and try again"));
                }

                System.IO.Directory.Move(fullOriginalPath, fullDestinationPath);
            }
            else
            {
                // file move
                var fullOriginalPath = this.GetAbsoluteFilePath(relativePathToDirectory, fileName);
                if (!ResolvedPathIsValid(fullOriginalPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return StatusCode(403);
                }

                var fullDestinationPath = this.GetAbsoluteFilePath(relativePathToDestDirectory, fileName);
                if (!ResolvedPathIsValid(fullDestinationPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return StatusCode(403);
                }

                if (!System.IO.File.Exists(fullOriginalPath))
                {
                    return NotFound(new ApiError("The file you are requesting to move does not exist"));
                }

                if (System.IO.File.Exists(fullDestinationPath))
                {
                    // In "real" production circumstances, I would figure out if we can just replace the file or increment its filename
                    return Conflict(new ApiError("There is already a file in the destination directory with that name. Delete that file and try again"));
                }

                System.IO.File.Move(fullOriginalPath, fullDestinationPath);
            }
            return Ok();
        }

        [HttpPut]
        [Route("copy")]
        // Extra
        public ActionResult Copy(string relativePathToDirectory, string fileName, string copyName)
        {
            if (string.IsNullOrEmpty(relativePathToDirectory))
            {
                relativePathToDirectory = "";
            }

            if (string.IsNullOrEmpty(fileName))
            {
                // directory move
                if (string.IsNullOrEmpty(relativePathToDirectory))
                {
                    // You cannot copy the root directory
                    return StatusCode(403);
                }
                
                var fullOriginalPath = this.GetAbsoluteDirectoryPath(relativePathToDirectory);
                if (!ResolvedPathIsValid(fullOriginalPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return StatusCode(403);
                }

                // Directory copy needs to go to the current "Parent"
                // So copying directory C to D with structure a/b/c/ would yield
                // a/b/c
                // a/b/d
                var fullDestinationPath = this.GetAbsoluteDirectoryPath(Path.Join(relativePathToDirectory, "../", copyName));
                if (!ResolvedPathIsValid(fullDestinationPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return StatusCode(403);
                }

                if (!System.IO.Directory.Exists(fullOriginalPath))
                {
                    return NotFound(new ApiError("The file you are requesting to move does not exist"));
                }

                if (System.IO.Directory.Exists(fullDestinationPath))
                {
                    // In "real" production circumstances, I would figure out if we can just replace the file or increment its filename
                    return Conflict(new ApiError("There is already a file in the destination directory with that name. Delete that file and try again"));
                }

                FileHelper.DirectoryCopy(fullOriginalPath, fullDestinationPath);
            }
            else
            {
                // file move
                var fullOriginalPath = this.GetAbsoluteFilePath(relativePathToDirectory, fileName);
                if (!ResolvedPathIsValid(fullOriginalPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return StatusCode(403);
                }

                var fullDestinationPath = this.GetAbsoluteFilePath(relativePathToDirectory, copyName);
                if (!ResolvedPathIsValid(fullDestinationPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return StatusCode(403);
                }

                if (!System.IO.File.Exists(fullOriginalPath))
                {
                    return NotFound(new ApiError("The file you are requesting to move does not exist"));
                }

                if (System.IO.File.Exists(fullDestinationPath))
                {
                    // In "real" production circumstances, I would figure out if we can just replace the file or increment its filename
                    return Conflict(new ApiError("There is already a file in the destination directory with that name. Delete that file and try again"));
                }

                System.IO.File.Copy(fullOriginalPath, fullDestinationPath);
            }

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
        

        // https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        public static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath);
            }
        }
    }
}