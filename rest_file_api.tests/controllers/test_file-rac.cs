using System;
using Xunit;
using rest_file_api.controllers;
using Moq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using rest_file_api.models;
using System.Collections.Generic;

namespace rest_file_api.tests.controllers
{
    public class FileRestApiController
    {
        [Fact]
        public void Get_DirectoryDoesntExist_NotFoundAsync()
        {
            // arrange
            const string path = "this/path/does/not/exist";
            var stubFileProvider = new Mock<IFileProvider>();
            var mockDirectory = new Mock<IDirectoryContents>();
            mockDirectory.Setup(x => x.Exists).Returns(false);

            stubFileProvider.Setup(x => x.GetDirectoryContents(path)).Returns(mockDirectory.Object);
            var mockConfigProvider = new Mock<IConfiguration>();

            var sut = new rest_file_api.controllers.FileRestApiController(mockConfigProvider.Object, stubFileProvider.Object);

            // act
            var result = sut.Get(path);

            // assert
            var actionResult = Assert.IsType<ActionResult<ApiDirectory>>(result);
            Assert.IsAssignableFrom<NotFoundObjectResult>(actionResult.Result);
        }

        [Fact]
        public void Get_NullDirectory_ReturnsRoot()
        {
            // arrange
            const string path = "";
            var stubFileProvider = new Mock<IFileProvider>();
            var mockDirectory = new Mock<IDirectoryContents>();
            mockDirectory.Setup(x => x.Exists).Returns(true);
            mockDirectory.Setup(x => x.GetEnumerator()).Returns(new List<IFileInfo>().GetEnumerator());

            stubFileProvider.Setup(x => x.GetDirectoryContents(path)).Returns(mockDirectory.Object);
            var mockConfigProvider = new Mock<IConfiguration>();

            var sut = new rest_file_api.controllers.FileRestApiController(mockConfigProvider.Object, stubFileProvider.Object);

            // act
            var result = sut.Get(null);

            // assert
            var actionResult = Assert.IsType<ActionResult<ApiDirectory>>(result);
            var apiResult = Assert.IsType<ApiDirectory>(actionResult.Value);
            Assert.NotNull(apiResult);
        }


        [Fact]
        public void Get_DirectoryExists_ReturnsDirectory()
        {
            // arrange
            const string path = "this/path/exists";
            var stubFileProvider = new Mock<IFileProvider>();
            var mockDirectory = new Mock<IDirectoryContents>();
            mockDirectory.Setup(x => x.Exists).Returns(true);
            mockDirectory.Setup(x => x.GetEnumerator()).Returns(new List<IFileInfo>().GetEnumerator());

            stubFileProvider.Setup(x => x.GetDirectoryContents(path)).Returns(mockDirectory.Object);
            var mockConfigProvider = new Mock<IConfiguration>();

            var sut = new rest_file_api.controllers.FileRestApiController(mockConfigProvider.Object, stubFileProvider.Object);

            // act
            var result = sut.Get(path);

            // assert
            var actionResult = Assert.IsType<ActionResult<ApiDirectory>>(result);
            var apiResult = Assert.IsType<ApiDirectory>(actionResult.Value);
            Assert.NotNull(apiResult);
        }

        [Fact(Skip = "Scaffold test")]
        public void Download_FileDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Download_Exists_ReturnsFile()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Upload_NoFiles_BadRequest()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Upload_FileAlreadyExists_Conflict()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Upload_Valid_Uploaded()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Search_()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Delete_FileDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Delete_DirectoryDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Delete_Directory_Deletes()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Delete_File_Deletes()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Move_FileDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Move_DirectoryDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Move_FileExistsAtDestination_Conflict()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Move_DirectoryExistsAtDestination_Conflict()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Move_File_MovesFile()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Move_Directory_MovesDirectory()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Copy_FileDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Copy_DirectoryDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Copy_FileExistsAtDestination_Conflict()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Copy_DirectoryExistsAtDestination_Conflict()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Copy_File_CopiesFile()
        {
            Assert.True(false);
        }

        [Fact(Skip = "Scaffold test")]
        public void Copy_Directory_CopiesDirectory()
        {
            Assert.True(false);
        }
    }
}
