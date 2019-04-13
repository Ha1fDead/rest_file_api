using System;
using Xunit;
using rest_file_api.controllers;
using Moq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace rest_file_api.tests.controllers
{
    public class FileRestApiController
    {
        [Fact]
        public async Task Get_DirectoryDoesntExist_NotFoundAsync()
        {
            // arrange
            var mockFileProvider = new Mock<IFileProvider>();
            var mockConfigProvider = new Mock<IConfiguration>();

            var sut = new FileController(mockConfigProvider.Object, mockFileProvider.Object);

            // act
            var directory = await sut.Get("this/does/not/exist");

            // assert
            Assert.True(false);
        }

        [Fact]
        public void Get_RelativePathGoesUp_DoesNotGoUp()
        {
            Assert.True(false);
        }

        [Fact]
        public void Get_DirectoryExists_ReturnsDirectory()
        {
            Assert.True(false);
        }

        [Fact]
        public void Download_FileDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact]
        public void Download_RelativePathGoesUp_DoesNotGoUp()
        {
            Assert.True(false);
        }

        [Fact]
        public void Download_Exists_ReturnsFile()
        {
            Assert.True(false);
        }

        [Fact]
        public void Upload_NoFiles_BadRequest()
        {
            Assert.True(false);
        }

        [Fact]
        public void Upload_FileAlreadyExists_Conflict()
        {
            Assert.True(false);
        }

        [Fact]
        public void Upload_Valid_Uploaded()
        {
            Assert.True(false);
        }

        [Fact]
        public void Search_()
        {
            Assert.True(false);
        }

        [Fact]
        public void Delete_FileDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact]
        public void Delete_DirectoryDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact]
        public void Delete_RelativePathGoesUp_DoesNotGoUp()
        {
            Assert.True(false);
        }

        [Fact]
        public void Delete_Directory_Deletes()
        {
            Assert.True(false);
        }

        [Fact]
        public void Delete_File_Deletes()
        {
            Assert.True(false);
        }

        [Fact]
        public void Move_FileDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact]
        public void Move_DirectoryDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact]
        public void Move_RelativePathGoesUp_DoesNotGoUp()
        {
            Assert.True(false);
        }

        [Fact]
        public void Move_FileExistsAtDestination_Conflict()
        {
            Assert.True(false);
        }

        [Fact]
        public void Move_DirectoryExistsAtDestination_Conflict()
        {
            Assert.True(false);
        }

        [Fact]
        public void Move_File_MovesFile()
        {
            Assert.True(false);
        }

        [Fact]
        public void Move_Directory_MovesDirectory()
        {
            Assert.True(false);
        }

        [Fact]
        public void Copy_FileDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact]
        public void Copy_DirectoryDoesntExist_NotFound()
        {
            Assert.True(false);
        }

        [Fact]
        public void Copy_RelativePathGoesUp_DoesNotGoUp()
        {
            Assert.True(false);
        }

        [Fact]
        public void Copy_FileExistsAtDestination_Conflict()
        {
            Assert.True(false);
        }

        [Fact]
        public void Copy_DirectoryExistsAtDestination_Conflict()
        {
            Assert.True(false);
        }

        [Fact]
        public void Copy_File_CopiesFile()
        {
            Assert.True(false);
        }

        [Fact]
        public void Copy_Directory_CopiesDirectory()
        {
            Assert.True(false);
        }
    }
}
