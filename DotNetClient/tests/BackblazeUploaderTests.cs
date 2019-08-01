using System;
using System.Net.Http;
using Xunit;


namespace StableCube.Backblaze.DotNetClient.Tests
{
    public class BackblazeUploaderTests
    {
        private static string _keyId = "";
        private static string _appKey = "";
        private static string _bucketId = "";
        private IB2Client _client;
        private IBackblazeUploader _uploader;

        public BackblazeUploaderTests()
        {
            var http = new HttpClient();
            _client = new B2Client(http);
            _uploader = new BackblazeUploader(_client);
        }

        [Fact]
        public async void Should_Upload_Large_File_Dynamically()
        {
            var auth = await _client.AuthorizeAsync(_keyId, _appKey);

            var path = "/home/zboyet/Documents/TestMedia/Videos/BigBuckBunny_640x360.m4v";
            var fileData = await _uploader.UploadDynamicAsync(
                auth: auth, 
                file: new UploadFile(
                    bucketId: _bucketId,
                    sourceFilePath: path,
                    destinationFilename: "test-dynamic-large.m4v"
                ), 
                retryTimeoutCount: 5,
                progressData: new Progress<TransferProgress>((TransferProgress progress) => {
                    Console.WriteLine($"got TransferProgress BytesTransferred: {progress.BytesTransferred}, TotalBytes: {progress.TotalBytes}");
                })
            );

            Assert.StartsWith("upload", fileData.Action);
        }

         [Fact]
        public async void Should_Upload_Small_File_Dynamically()
        {
            var auth = await _client.AuthorizeAsync(_keyId, _appKey);

            var path = "/home/zboyet/Documents/TestMedia/Videos/V-Mpeg2_A-Mpeg2_Zelda.mpeg";
            var fileData = await _uploader.UploadDynamicAsync(
                auth: auth, 
                file: new UploadFile(
                    bucketId: _bucketId,
                    sourceFilePath: path,
                    destinationFilename: "test-dynamic-large.m4v"
                ), 
                retryTimeoutCount: 5
            );

            Assert.StartsWith("upload", fileData.Action);
        }
    }
}