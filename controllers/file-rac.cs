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
        [HttpGet]
        public async Task<ActionResult<ApiDirectory>> Get()
        {
            return FileHelper.GetDirectoryInfo(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Delete(string path_to_file)
        {
            return Ok();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiFile>> Upload(string filename)
        {
            var createdFile = new ApiFile();
            return CreatedAtAction("something", 4, createdFile);
        }
    }

    public static class FileHelper
    {
        public static ApiDirectory GetDirectoryInfo(DirectoryInfo info)
        {

            var allfiles = new List<ApiFile>();
            foreach(var file in info.GetFiles())
            {
                allfiles.Add(GetFileInfo(file));
            }

            var directory = new ApiDirectory
            {
                Name = info.FullName,
                Files = allfiles,
                SubDirectories = new List<string>()
            };

            return directory;
        }

        public static ApiFile GetFileInfo(FileInfo info)
        {
            var file = new ApiFile
            {
                Name = info.FullName,
                DateCreated = info.CreationTimeUtc,
                DateModified = info.LastWriteTimeUtc,
                SizeBytes = info.Length
            };

            return file;
        }

        // Oof directory walking can get complex. Mapping my home directory takes FOR-EH-VER
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