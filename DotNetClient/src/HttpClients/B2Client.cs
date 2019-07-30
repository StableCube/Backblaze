using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class B2Client : IB2Client
    {
        private HttpClient _client;

        public B2Client(
            HttpClient client
        )
        {
            _client = client;
            _client.Timeout = TimeSpan.FromMinutes(20);
        }

        public async Task<Authorization> AuthorizeAsync(
            string keyId, 
            string applicationKey,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            string endpoint = "https://api.backblazeb2.com/b2api/v2/b2_authorize_account";
            string creds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{applicationKey}"));

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", creds);

            var result = await _client.GetAsync(endpoint, cancellationToken);
            result.EnsureSuccessStatusCode();

            string resultJson = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Authorization>(resultJson);
        }

        private StringContent SerializeToJsonContent(object input)
        {
            return new StringContent(JsonConvert.SerializeObject(input, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }), Encoding.UTF8, "application/json");
        }

        public async Task<UploadUrl> GetUploadUrlAsync(
            Authorization auth, 
            string bucketId,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var body = SerializeToJsonContent(new UploadUrlInput()
            {
                BucketId = bucketId
            });

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{auth.ApiUrl}/b2api/v2/b2_get_upload_url"),
                Content = body
            };

            request.Headers.TryAddWithoutValidation("Authorization", auth.AuthorizationToken);

            var response = await _client.SendAsync(request, cancellationToken);
            string responseJson = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
                ErrorHelper.ThrowException(responseJson);

            return JsonConvert.DeserializeObject<UploadUrl>(responseJson);
        }

        /// <summary>
        /// Upload a file without any error recovery
        /// </summary>
        public async Task<FileData> UploadSmallFileAsync(
            UploadUrl uploadUrl, 
            string sourcePath, 
            string destinationFilename,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            using (var fs = File.OpenRead(sourcePath))
            {
                byte[] fileData = await File.ReadAllBytesAsync(sourcePath);
                string fileHash = GetFileHash(fileData);

                using (var fileContent = new ByteArrayContent(fileData))
                {
                    fileContent.Headers.ContentLength = fs.Length;
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("b2/x-auto");

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(uploadUrl.Url),
                        Headers = {
                            { "X-Bz-File-Name", destinationFilename },
                            { "X-Bz-Content-Sha1", fileHash },
                        },
                        Content = fileContent
                    };

                    request.Headers.TryAddWithoutValidation("Authorization", uploadUrl.AuthorizationToken);

                    var result = await _client.SendAsync(request, cancellationToken);
                    string resultJson = await result.Content.ReadAsStringAsync();

                    if(!result.IsSuccessStatusCode)
                        ErrorHelper.ThrowException(resultJson);
                        
                    return JsonConvert.DeserializeObject<FileData>(resultJson);
                }
            }
        }

        /// <summary>
        /// Upload a file with attempted recovery from errors
        /// </summary>
        public async Task<FileData> UploadSmallFileAsync(
            Authorization auth,
            string bucketId, 
            string sourcePath, 
            string destinationFilename,
            int retryTimeoutCount = 5,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            int retryCount = 0;

            Retry:
            var uploadUrl = await GetUploadUrlAsync(auth, bucketId, cancellationToken);

            FileData uploadData;
            try
            {
                uploadData = await UploadSmallFileAsync(uploadUrl, sourcePath, destinationFilename, cancellationToken);
            }
            catch (B2Exception e)
            {
                if(ErrorHelper.IsRecoverableException(e))
                {
                    if(retryCount > retryTimeoutCount)
                    {
                        throw new B2RetryTimeoutException($"Hit retry limit of {retryTimeoutCount}", e);
                    }

                    retryCount++;
                    goto Retry;
                }
                else
                {
                    throw e;
                }
            }

            return uploadData;
        }

        public async Task<FileData> StartLargeFileUploadAsync(
            Authorization auth, 
            string bucketId,
            string destinationFilename,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var body = SerializeToJsonContent(new StartLargeFileUploadInput()
            {
                BucketId = bucketId,
                FileName = destinationFilename,
                ContentType = "b2/x-auto",
            });

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{auth.ApiUrl}/b2api/v2/b2_start_large_file"),
                Content = body
            };

            request.Headers.TryAddWithoutValidation("Authorization", auth.AuthorizationToken);

            var response = await _client.SendAsync(request, cancellationToken);
            string responseJson = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
                ErrorHelper.ThrowException(responseJson);

            return JsonConvert.DeserializeObject<FileData>(responseJson);
        }

        public async Task<FileData> FinishLargeFileUploadAsync(
            Authorization auth, 
            string fileId,
            IEnumerable<string> partHashes,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var body = SerializeToJsonContent(new FinishLargeFileUploadInput()
            {
                FileId = fileId,
                PartSha1Array = (string[])partHashes
            });

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{auth.ApiUrl}/b2api/v2/b2_finish_large_file"),
                Content = body
            };

            request.Headers.TryAddWithoutValidation("Authorization", auth.AuthorizationToken);

            var response = await _client.SendAsync(request, cancellationToken);
            string responseJson = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
                ErrorHelper.ThrowException(responseJson);

            return JsonConvert.DeserializeObject<FileData>(responseJson);
        }

        public async Task<UploadPartUrl> GetUploadPartUrlAsync(
            Authorization auth,
            FileData uploadData,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var body = SerializeToJsonContent(new UploadPartUrlInput()
            {
                FileId = uploadData.FileId
            });

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{auth.ApiUrl}/b2api/v2/b2_get_upload_part_url"),
                Content = body
            };

            request.Headers.TryAddWithoutValidation("Authorization", auth.AuthorizationToken);

            var response = await _client.SendAsync(request, cancellationToken);
            string responseJson = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
                ErrorHelper.ThrowException(responseJson);

            return JsonConvert.DeserializeObject<UploadPartUrl>(responseJson);
        }

        public async Task<UploadedPart> UploadLargeFilePartAsync(
            UploadPartUrl uploadUrl, 
            string sourcePath,
            int partNumber,
            long maxPartSize,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            

            using (var fs = File.OpenRead(sourcePath))
            {
                long totalFileSize = fs.Length;
                int totalFileParts = (int)Math.Ceiling((double)totalFileSize / (double)maxPartSize);
                if(totalFileParts < 2)
                    throw new B2LargeFileNotNeededException();

                if(partNumber < 0 || partNumber > totalFileParts)
                    throw new ArgumentOutOfRangeException($"partNumber: {partNumber.ToString()}");

                long partSize = maxPartSize;
                if(partNumber == totalFileParts)
                    partSize = totalFileSize - ((totalFileParts - 1) * maxPartSize);

                int beginOffset = (int)((partNumber - 1) * maxPartSize);

                byte[] filePartData = new byte[partSize];
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    reader.BaseStream.Seek(beginOffset, SeekOrigin.Begin);
                    reader.Read(filePartData, 0, (int)partSize);

                    string fileHash = GetFileHash(filePartData);

                    using (var fileContent = new ByteArrayContent(filePartData))
                    {
                        fileContent.Headers.ContentLength = partSize;
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("b2/x-auto");

                        var request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Post,
                            RequestUri = new Uri(uploadUrl.Url),
                            Headers = {
                                { "X-Bz-Part-Number", partNumber.ToString() },
                                { "X-Bz-Content-Sha1", fileHash },
                            },
                            Content = fileContent
                        };

                        request.Headers.TryAddWithoutValidation("Authorization", uploadUrl.AuthorizationToken);

                        var result = await _client.SendAsync(request, cancellationToken);
                        string resultJson = await result.Content.ReadAsStringAsync();

                        if(!result.IsSuccessStatusCode)
                        {
                            ErrorHelper.ThrowException(resultJson);
                        }

                        return JsonConvert.DeserializeObject<UploadedPart>(resultJson);
                    }
                }
            }
        }


        public async Task<UploadedPart> UploadLargeFilePartAsync(
            Authorization auth,
            FileData fileData,
            string sourcePath,
            int partNumber,
            long maxPartSize,
            int retryTimeoutCount = 5,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            int retryCount = 0;

            Retry:
            var uploadUrl = await GetUploadPartUrlAsync(auth, fileData, cancellationToken);

            UploadedPart uploadPart;
            try
            {
                uploadPart = await UploadLargeFilePartAsync(uploadUrl, sourcePath, partNumber, maxPartSize, cancellationToken);
            }
            catch (B2Exception e)
            {
                if(ErrorHelper.IsRecoverableException(e))
                {
                    if(retryCount > retryTimeoutCount)
                    {
                        throw new B2RetryTimeoutException($"Hit retry limit of {retryTimeoutCount}", e);
                    }

                    retryCount++;
                    goto Retry;
                }
                else
                {
                    throw e;
                }
            }

            return uploadPart;
        }

        /// <summary>
        /// Probes the file size and automatically does a small or large file upload.
        /// 
        /// Retries are attempted on recoverable errors
        /// </summary>
        public async Task<FileData> UploadDynamicAsync(
            Authorization auth,
            string sourcePath,
            string bucketId,
            string destinationFilename,
            int retryTimeoutCount = 5,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            long fileSize = new FileInfo(sourcePath).Length;

            if(fileSize > auth.RecommendedPartSize)
            {
                int partCount = (int)Math.Ceiling((double)fileSize / (double)auth.RecommendedPartSize);
                var initData = await StartLargeFileUploadAsync(auth, bucketId, destinationFilename, cancellationToken);
                string[] partHashes = new string[partCount];
                Task<UploadedPart>[] partTasks = new Task<UploadedPart>[partCount];

                for (int i = 0; i < partCount; i++)
                {
                    partTasks[i] =  UploadLargeFilePartAsync(
                            auth: auth,
                            fileData: initData,
                            sourcePath: sourcePath,
                            partNumber: i + 1,
                            maxPartSize: auth.RecommendedPartSize,
                            retryTimeoutCount: retryTimeoutCount,
                            cancellationToken: cancellationToken
                    );
                }

                UploadedPart[] uploadedParts = await Task.WhenAll(partTasks);
                for (int i = 0; i < partCount; i++)
                {
                    partHashes[i] = uploadedParts[i].ContentSha1;
                }

                return await FinishLargeFileUploadAsync(auth, initData.FileId, partHashes);
            }
            else
            {
                return await UploadSmallFileAsync(
                    auth: auth, 
                    bucketId: bucketId, 
                    sourcePath: sourcePath, 
                    destinationFilename: destinationFilename, 
                    retryTimeoutCount: retryTimeoutCount, 
                    cancellationToken: cancellationToken
                );
            }
        }

        private string GetFileHash(byte[] fileData)
        {
            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] data = sha1Hash.ComputeHash(fileData);
                var sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                    sBuilder.Append(data[i].ToString("x2"));
                
                return sBuilder.ToString();
            }
        }
    }
}