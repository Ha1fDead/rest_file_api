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
using System.Net;
using rest_file_api.models.actions;
using rest_file_api.logic;

namespace rest_file_api.controllers
{

    [Produces("application/json")]
    [Route("api/file")]
    [ApiController]
    public class FileRestApiController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IFileProvider _fileProvider;

        public FileRestApiController(
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

            relativePathToDirectory = WebUtility.UrlDecode(relativePathToDirectory);

            var directoryInfo = _fileProvider.GetDirectoryContents(relativePathToDirectory);
            if (!directoryInfo.Exists) 
            {
                return NotFound(new ApiError("The directory requested does not exist"));
            }

            return FileLogic.GetDirectoryInfo(directoryInfo, relativePathToDirectory);
        }

        [HttpGet]
        [HttpPost]
        // Best to leave this as URL-encoded so native browser functionality can take over
        [Route("download/{*relativePathToFile}")]
        public async Task<ActionResult> Download(string relativePathToFile)
        {
            if (string.IsNullOrEmpty(relativePathToFile))
            {
                return BadRequest(new ApiError("You must specify a valid path to the file"));
            }

            relativePathToFile = WebUtility.UrlDecode(relativePathToFile);
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
        public ActionResult Delete([FromBody] ApiDeleteFile model)
        {
            if (string.IsNullOrEmpty(model.RelativePathToDirectory))
            {
                model.RelativePathToDirectory = "";
            }

            if (string.IsNullOrWhiteSpace(model.FileName))
            {
                var fullDestPath = this.GetAbsoluteDirectoryPath(model.RelativePathToDirectory);
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
                var fileInfo = _fileProvider.GetFileInfo(Path.Join(model.RelativePathToDirectory, model.FileName));
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
        public ActionResult Move([FromBody] ApiMoveFile model)
        {
            if (string.IsNullOrEmpty(model.RelativePathToDirectory))
            {
                model.RelativePathToDirectory = "";
            }

            if (string.IsNullOrEmpty(model.RelativePathToDestDirectory))
            {
                model.RelativePathToDestDirectory = "";
            }

            if (string.IsNullOrEmpty(model.FileName))
            {
                // directory move
                var fullOriginalPath = this.GetAbsoluteDirectoryPath(model.RelativePathToDirectory);
                if (!ResolvedPathIsValid(fullOriginalPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return StatusCode(403);
                }

                var fullDestinationPath = this.GetAbsoluteDirectoryPath(model.RelativePathToDestDirectory);
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
                var fullOriginalPath = this.GetAbsoluteFilePath(model.RelativePathToDirectory, model.FileName);
                if (!ResolvedPathIsValid(fullOriginalPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return StatusCode(403);
                }

                var fullDestinationPath = this.GetAbsoluteFilePath(model.RelativePathToDestDirectory, "");
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

                if (!System.IO.Directory.Exists(Path.GetDirectoryName(fullDestinationPath)))
                {
                    return NotFound(new ApiError("The directory you are trying to move the file into does not exist"));
                }

                System.IO.File.Move(fullOriginalPath, fullDestinationPath);
            }
            return Ok();
        }

        [HttpPut]
        [Route("copy")]
        // Extra
        public ActionResult Copy([FromBody] ApiCopyFile model)
        {
            if (string.IsNullOrEmpty(model.RelativePathToDirectory))
            {
                model.RelativePathToDirectory = "";
            }

            if (string.IsNullOrEmpty(model.FileName))
            {
                // directory move
                if (string.IsNullOrEmpty(model.RelativePathToDirectory))
                {
                    // You cannot copy the root directory
                    return StatusCode(403);
                }
                
                var fullOriginalPath = this.GetAbsoluteDirectoryPath(model.RelativePathToDirectory);
                if (!ResolvedPathIsValid(fullOriginalPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return StatusCode(403);
                }

                // Directory copy needs to go to the current "Parent"
                // So copying directory C to D with structure a/b/c/ would yield
                // a/b/c
                // a/b/d
                var fullDestinationPath = this.GetAbsoluteDirectoryPath(Path.Join(model.RelativePathToDirectory, "../", model.CopyName));
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

                FileLogic.DirectoryCopy(fullOriginalPath, fullDestinationPath);
            }
            else
            {
                // file move
                var fullOriginalPath = this.GetAbsoluteFilePath(model.RelativePathToDirectory, model.FileName);
                if (!ResolvedPathIsValid(fullOriginalPath))
                {
                    // User may be attempting to view "Up" directories -- app should only let people view "Down"
                    return StatusCode(403);
                }

                var fullDestinationPath = this.GetAbsoluteFilePath(model.RelativePathToDirectory, model.CopyName);
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
}