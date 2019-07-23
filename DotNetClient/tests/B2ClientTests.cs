using System;
using System.Net.Http;
using Xunit;


namespace StableCube.Backblaze.DotNetClient.Tests
{
    public class B2ClientTests
    {
        private static string _keyId = "";
        private static string _appKey = "";
        private static string _bucketId = "";

        public B2ClientTests()
        {
        }

        [Fact]
        public async void Should_Authenticate()
        {
            var http = new HttpClient();
            var client = new B2Client(http);
            var auth = await client.AuthorizeAsync(_keyId, _appKey);

            Assert.True(auth.HasWriteFilePermission());
        }

        [Fact]
        public async void Should_Get_Upload_Url()
        {
            var http = new HttpClient();
            var client = new B2Client(http);
            var auth = await client.AuthorizeAsync(_keyId, _appKey);
            var uploadUrl = await client.GetUploadUrlAsync(auth, _bucketId);

            Assert.StartsWith("https", uploadUrl.Url);
        }

        [Fact]
        public async void Should_Upload_Small_File()
        {
            var http = new HttpClient();
            var client = new B2Client(http);
            var auth = await client.AuthorizeAsync(_keyId, _appKey);
            var uploadUrl = await client.GetUploadUrlAsync(auth, _bucketId);

            var path = "/home/zboyet/Documents/TestMedia/Videos/V-Mpeg2_A-Mpeg2_Zelda.mpeg";
            var uploadResult = await client.UploadSmallFileAsync(uploadUrl, path, "test.mpeg");

            Assert.StartsWith("upload", uploadResult.Action);
        }

        [Fact]
        public async void Should_Start_Large_File_Upload()
        {
            var http = new HttpClient();
            var client = new B2Client(http);
            var auth = await client.AuthorizeAsync(_keyId, _appKey);
            var uploadInit = await client.StartLargeFileUploadAsync(auth, _bucketId, "test-large.m4v");

            Assert.Equal("start", uploadInit.Action);
        }

        [Fact]
        public async void Should_Get_Large_File_Upload_Url()
        {
            var http = new HttpClient();
            var client = new B2Client(http);
            var auth = await client.AuthorizeAsync(_keyId, _appKey);
            var uploadInit = await client.StartLargeFileUploadAsync(auth, _bucketId, "test-large.m4v");
            var uploadUrl = await client.GetUploadPartUrlAsync(auth, uploadInit);

            Assert.StartsWith("https", uploadUrl.Url);
        }

        [Fact]
        public async void Should_Upload_Large_File_Part()
        {
            var http = new HttpClient();
            var client = new B2Client(http);
            var auth = await client.AuthorizeAsync(_keyId, _appKey);
            var uploadInit = await client.StartLargeFileUploadAsync(auth, _bucketId, "test-large.m4v");
            var uploadUrl = await client.GetUploadPartUrlAsync(auth, uploadInit);

            var path = "/home/zboyet/Documents/TestMedia/Videos/BigBuckBunny_640x360.m4v";
            var upload = await client.UploadLargeFilePartAsync(uploadUrl, path, 1, auth.RecommendedPartSize);

            Assert.Equal(1, upload.PartNumber);
        }

        [Fact]
        public async void Should_Upload_Complete_Large_File()
        {
            var http = new HttpClient();
            var client = new B2Client(http);
            var auth = await client.AuthorizeAsync(_keyId, _appKey);
            var uploadInit = await client.StartLargeFileUploadAsync(auth, _bucketId, "test-large.m4v");
            var uploadUrl = await client.GetUploadPartUrlAsync(auth, uploadInit);

            var path = "/home/zboyet/Documents/TestMedia/Videos/BigBuckBunny_640x360.m4v";
            var part1 = await client.UploadLargeFilePartAsync(uploadUrl, path, 1, auth.RecommendedPartSize);
            var part2 = await client.UploadLargeFilePartAsync(uploadUrl, path, 2, auth.RecommendedPartSize);

            string[] partHashes = new string[2];
            partHashes[0] = part1.ContentSha1;
            partHashes[1] = part2.ContentSha1;

            var fileData = await client.FinishLargeFileUploadAsync(auth, uploadInit.FileId, partHashes);

            Assert.StartsWith("upload", fileData.Action);
        }

        [Fact]
        public async void Should_Upload_Large_File_Dynamically()
        {
            var http = new HttpClient();
            var client = new B2Client(http);
            var auth = await client.AuthorizeAsync(_keyId, _appKey);

            var path = "/home/zboyet/Documents/TestMedia/Videos/BigBuckBunny_640x360.m4v";
            var fileData = await client.UploadDynamic(
                auth: auth, 
                sourcePath: path, 
                bucketId: _bucketId, 
                destinationFilename: "test-dynamic-large.m4v",
                retryTimeoutCount: 5
            );

            Assert.StartsWith("upload", fileData.Action);
        }

         [Fact]
        public async void Should_Upload_Small_File_Dynamically()
        {
            var http = new HttpClient();
            var client = new B2Client(http);
            var auth = await client.AuthorizeAsync(_keyId, _appKey);

            var path = "/home/zboyet/Documents/TestMedia/Videos/V-Mpeg2_A-Mpeg2_Zelda.mpeg";
            var fileData = await client.UploadDynamic(
                auth: auth, 
                sourcePath: path, 
                bucketId: _bucketId, 
                destinationFilename: "test-dynamic-small.mpeg",
                retryTimeoutCount: 5
            );

            Assert.StartsWith("upload", fileData.Action);
        }
    }
}