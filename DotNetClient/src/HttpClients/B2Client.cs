using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using System.Timers;
using Newtonsoft.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class B2Client : IB2Client
    {
        protected HttpClient _client;

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
            IProgress<FileProgress> progressData = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            using (var fs = File.OpenRead(sourcePath))
            {
                string fileHash = GetFileHash(fs);

                fs.Seek(0, SeekOrigin.Begin);

                using (var fileContent = new StreamContent(fs))
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

                    var progressTimer = new System.Timers.Timer();
                    if(progressData != null)
                    {
                        RunSmallUploadProgressMonitor(progressTimer, fs, destinationFilename, progressData);
                    }

                    using(var result = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        string resultJson = await result.Content.ReadAsStringAsync();
                        progressTimer.Enabled = false;

                        if(!result.IsSuccessStatusCode)
                            ErrorHelper.ThrowException(resultJson);
                            
                        return JsonConvert.DeserializeObject<FileData>(resultJson);
                    };
                }
            }
        }

        private void RunSmallUploadProgressMonitor(
            System.Timers.Timer progressTimer, 
            Stream partStream,
            string destinationFilename,
            IProgress<FileProgress> progressData
        )
        {
            long lastReport = 0;
            progressTimer.Elapsed += new ElapsedEventHandler((object source, ElapsedEventArgs e) => {
                if(partStream.Position == partStream.Length)
                    progressTimer.Enabled = false;

                if(lastReport == partStream.Position)
                    return;

                progressData?.Report(new FileProgress(
                    filename: destinationFilename,
                    totalBytes: partStream.Length,
                    bytesTransferred: partStream.Position
                ));

                lastReport = partStream.Position;
            });
            
            progressTimer.Interval = 500;
            progressTimer.Enabled = true;
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
            IProgress<FileProgress> progressData = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            int retryCount = 0;

            Retry:
            var uploadUrl = await GetUploadUrlAsync(auth, bucketId, cancellationToken);

            FileData uploadData = null;
            try
            {
                uploadData = await UploadSmallFileAsync(uploadUrl, sourcePath, destinationFilename, progressData, cancellationToken);
            }
            catch (Exception e)
            {
                if(ErrorHelper.IsRecoverableException(e))
                {
                    if(retryCount > retryTimeoutCount)
                        throw new B2RetryTimeoutException($"Hit retry limit of {retryTimeoutCount}", e);

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
            Stream partStream,
            string destinationFilename,
            int partNumber,
            IProgress<FilePartProgress> progressData = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            if(partStream == null)
                throw new NullReferenceException("partStream");

            if(!partStream.CanRead)
                throw new InvalidDataException();

            string fileHash = GetFileHash(partStream);
            partStream.Seek(0, SeekOrigin.Begin);
            
            var progressTimer = new System.Timers.Timer();
            if(progressData != null)
            {
                RunPartProgressMonitor(progressTimer, partStream, destinationFilename, progressData, partNumber);
            }

            using (var fileContent = new StreamContent(partStream))
            {
                fileContent.Headers.ContentLength = partStream.Length;
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

                using(var result = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    string resultJson = await result.Content.ReadAsStringAsync();
                    progressTimer.Enabled = false;

                    if(!result.IsSuccessStatusCode)
                        ErrorHelper.ThrowException(resultJson);

                    return JsonConvert.DeserializeObject<UploadedPart>(resultJson);
                };
            }
        }

        private void RunPartProgressMonitor(
            System.Timers.Timer progressTimer, 
            Stream partStream,
            string destinationFilename,
            IProgress<FilePartProgress> progressData,
            int partNumber
        )
        {
            long lastReport = 0;
            progressTimer.Elapsed += new ElapsedEventHandler((object source, ElapsedEventArgs e) => {
                if(partStream.Position == partStream.Length)
                    progressTimer.Enabled = false;

                if(lastReport == partStream.Position)
                    return;

                progressData?.Report(new FilePartProgress(
                    filename: destinationFilename,
                    partNumber: partNumber,
                    totalBytes: partStream.Length,
                    bytesTransferred: partStream.Position
                ));

                lastReport = partStream.Position;
            });
            progressTimer.Interval = 500;
            progressTimer.Enabled = true;
        }

        public async Task<UploadedPart> UploadLargeFilePartAsync(
            Authorization auth,
            FileData fileData,
            Stream partStream,
            int partNumber,
            int retryTimeoutCount = 5,
            IProgress<FilePartProgress> progressData = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            if(partStream == null)
                throw new NullReferenceException("partStream");

            if(!partStream.CanRead)
                throw new InvalidDataException();
            
            int retryCount = 0;

            Retry:
            var uploadUrl = await GetUploadPartUrlAsync(auth, fileData, cancellationToken);

            UploadedPart uploadPart = null;
            try
            {
                uploadPart = await UploadLargeFilePartAsync(uploadUrl, partStream, fileData.FileName, partNumber, progressData, cancellationToken);
            }
            catch (Exception e)
            {
                if(ErrorHelper.IsRecoverableException(e))
                {
                    if(retryCount > retryTimeoutCount)
                        throw new B2RetryTimeoutException($"Hit retry limit of {retryTimeoutCount}", e);

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

        private string GetFileHash(byte[] fileData)
        {
            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] hash = sha1Hash.ComputeHash(fileData);

                return BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
            }
        }

        private async Task<string> GetFileHashAsync(FileStream fileData, long index, long count)
        {
            byte[] buffer = new byte[count];
            await fileData.ReadAsync(buffer, (int)index, (int)count);

            return GetFileHash(buffer);
        }

        private string GetFileHash(Stream fileStream)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(fileStream);

                return BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
            }
        }
    }
}