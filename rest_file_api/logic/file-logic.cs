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


namespace rest_file_api.logic
{
    public static class FileLogic
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
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath);
            }
        }
    }
}