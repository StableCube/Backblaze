using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Timers;
using System.Text.Json;

namespace StableCube.Backblaze.DotNetClient
{
    public class B2ClientV2 : IB2ClientV2
    {
        protected HttpClient _client;
        private JsonSerializerOptions _writerOptions  = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public B2ClientV2(
            HttpClient client
        )
        {
            _client = client;
            _client.Timeout = TimeSpan.FromMinutes(20);
        }

        public async Task<BackblazeApiResponse<AuthorizationOutputDTO>> AuthorizeAsync(
            string keyId, 
            string applicationKey,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            string endpoint = "https://api.backblazeb2.com/b2api/v2/b2_authorize_account";
            string creds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{applicationKey}"));

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", creds);

            var result = await _client.GetAsync(endpoint, cancellationToken);
            string resultJson = await result.Content.ReadAsStringAsync();

            if(!result.IsSuccessStatusCode)
            {
                return new BackblazeApiResponse<AuthorizationOutputDTO>()
                {
                    Succeeded = result.IsSuccessStatusCode,
                    Error = JsonSerializer.Deserialize<B2ErrorResponseOutputDTO>(resultJson)
                };
            }

            return new BackblazeApiResponse<AuthorizationOutputDTO>()
            {
                Succeeded = result.IsSuccessStatusCode,
                Data = JsonSerializer.Deserialize<AuthorizationOutputDTO>(resultJson)
            };
        }

        private StringContent SerializeToJsonContent(object input)
        {
            return new StringContent(
                JsonSerializer.Serialize(input, input.GetType(), _writerOptions), 
                Encoding.UTF8, "application/json"
            );
        }

        public async Task<BackblazeApiResponse<UploadUrlOutputDTO>> GetUploadUrlAsync(
            AuthorizationOutputDTO auth, 
            string bucketId,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var body = SerializeToJsonContent(new UploadUrlInputDTO()
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
            {
                return new BackblazeApiResponse<UploadUrlOutputDTO>()
                {
                    Succeeded = response.IsSuccessStatusCode,
                    Error = JsonSerializer.Deserialize<B2ErrorResponseOutputDTO>(responseJson)
                };
            }

            return new BackblazeApiResponse<UploadUrlOutputDTO>()
            {
                Succeeded = response.IsSuccessStatusCode,
                Data = JsonSerializer.Deserialize<UploadUrlOutputDTO>(responseJson)
            };
        }

        /// <summary>
        /// Upload a file without any error recovery
        /// </summary>
        public async Task<BackblazeApiResponse<FileDataOutputDTO>> UploadSmallFileAsync(
            UploadUrlOutputDTO uploadUrl, 
            string sourcePath, 
            string destinationFilename,
            IProgress<FileProgress> progressData = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            using (var fs = File.OpenRead(sourcePath))
            {
                string fileHash = FileHasher.GetFileHash(fs);

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
                        {
                            return new BackblazeApiResponse<FileDataOutputDTO>()
                            {
                                Succeeded = result.IsSuccessStatusCode,
                                Error = JsonSerializer.Deserialize<B2ErrorResponseOutputDTO>(resultJson)
                            };
                        }

                        return new BackblazeApiResponse<FileDataOutputDTO>()
                        {
                            Succeeded = result.IsSuccessStatusCode,
                            Data = JsonSerializer.Deserialize<FileDataOutputDTO>(resultJson)
                        };
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
        public async Task<BackblazeApiResponse<FileDataOutputDTO>> UploadSmallFileAsync(
            AuthorizationOutputDTO auth,
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
            var uploadUrlResponse = await GetUploadUrlAsync(auth, bucketId, cancellationToken);
            if(!uploadUrlResponse.Succeeded)
            {
                return new BackblazeApiResponse<FileDataOutputDTO>()
                {
                    Succeeded = uploadUrlResponse.Succeeded,
                    Error = uploadUrlResponse.Error
                };
            }

            FileDataOutputDTO uploadData = null;
            var uploadDataResponse = await UploadSmallFileAsync(uploadUrlResponse.Data, sourcePath, destinationFilename, progressData, cancellationToken);
            if(uploadDataResponse.Succeeded)
            {
                uploadData = uploadDataResponse.Data;
            }
            else
            {
                if(ErrorHelper.IsRecoverableError(uploadDataResponse.Error))
                {
                    if(retryCount > retryTimeoutCount)
                    {
                        uploadDataResponse.Error.Message = $"Hit retry max on error: {uploadDataResponse.Error.Message}";

                        return new BackblazeApiResponse<FileDataOutputDTO>()
                        {
                            Succeeded = false,
                            Error = uploadDataResponse.Error
                        };
                    }

                    retryCount++;
                    goto Retry;
                }
                else
                {
                    return new BackblazeApiResponse<FileDataOutputDTO>()
                    {
                        Succeeded = false,
                        Error = uploadDataResponse.Error
                    };
                }
            }

            return new BackblazeApiResponse<FileDataOutputDTO>()
            {
                Succeeded = uploadDataResponse.Succeeded,
                Data = uploadData
            };
        }

        public async Task<BackblazeApiResponse<FileDataOutputDTO>> StartLargeFileAsync(
            AuthorizationOutputDTO auth, 
            string bucketId,
            string destinationFilename,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var body = SerializeToJsonContent(new StartLargeFileUploadInputDTO()
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
            {
                return new BackblazeApiResponse<FileDataOutputDTO>()
                {
                    Succeeded = response.IsSuccessStatusCode,
                    Error = JsonSerializer.Deserialize<B2ErrorResponseOutputDTO>(responseJson)
                };
            }

            return new BackblazeApiResponse<FileDataOutputDTO>()
            {
                Succeeded = response.IsSuccessStatusCode,
                Data = JsonSerializer.Deserialize<FileDataOutputDTO>(responseJson)
            };
        }

        public async Task<BackblazeApiResponse<FileDataOutputDTO>> FinishLargeFileAsync(
            AuthorizationOutputDTO auth, 
            string fileId,
            IEnumerable<string> partHashes,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var body = SerializeToJsonContent(new FinishLargeFileUploadInputDTO()
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
            {
                return new BackblazeApiResponse<FileDataOutputDTO>()
                {
                    Succeeded = response.IsSuccessStatusCode,
                    Error = JsonSerializer.Deserialize<B2ErrorResponseOutputDTO>(responseJson)
                };
            }

            return new BackblazeApiResponse<FileDataOutputDTO>()
            {
                Succeeded = response.IsSuccessStatusCode,
                Data = JsonSerializer.Deserialize<FileDataOutputDTO>(responseJson)
            };
        }

        public async Task<BackblazeApiResponse<UploadPartUrlOutputDTO>> GetUploadPartUrlAsync(
            AuthorizationOutputDTO auth,
            FileDataOutputDTO uploadData,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var body = SerializeToJsonContent(new UploadPartUrlInputDTO()
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
            {
                return new BackblazeApiResponse<UploadPartUrlOutputDTO>()
                {
                    Succeeded = response.IsSuccessStatusCode,
                    Error = JsonSerializer.Deserialize<B2ErrorResponseOutputDTO>(responseJson)
                };
            }

            return new BackblazeApiResponse<UploadPartUrlOutputDTO>()
            {
                Succeeded = response.IsSuccessStatusCode,
                Data = JsonSerializer.Deserialize<UploadPartUrlOutputDTO>(responseJson)
            };
        }

        private async Task<BackblazeApiResponse<UploadedPartOutputDTO>> UploadLargeFilePartAsync(
            UploadPartUrlOutputDTO uploadUrl, 
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

            string fileHash = FileHasher.GetFileHash(partStream);
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
                    {
                        return new BackblazeApiResponse<UploadedPartOutputDTO>()
                        {
                            Succeeded = result.IsSuccessStatusCode,
                            Error = JsonSerializer.Deserialize<B2ErrorResponseOutputDTO>(resultJson)
                        };
                    }

                    return new BackblazeApiResponse<UploadedPartOutputDTO>()
                    {
                        Succeeded = result.IsSuccessStatusCode,
                        Data = JsonSerializer.Deserialize<UploadedPartOutputDTO>(resultJson)
                    };
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

        public async Task<BackblazeApiResponse<UploadedPartOutputDTO>> UploadLargeFilePartAsync(
            AuthorizationOutputDTO auth,
            FileDataOutputDTO fileData,
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
            var uploadUrlResponse = await GetUploadPartUrlAsync(auth, fileData, cancellationToken);
            if(!uploadUrlResponse.Succeeded)
            {
                return new BackblazeApiResponse<UploadedPartOutputDTO>()
                {
                    Succeeded = uploadUrlResponse.Succeeded,
                    Error = uploadUrlResponse.Error
                };
            }

            UploadedPartOutputDTO uploadPart = null;
            var uploadPartResponse = await UploadLargeFilePartAsync(uploadUrlResponse.Data, partStream, fileData.FileName, partNumber, progressData, cancellationToken);
            if(uploadPartResponse.Succeeded)
            {
                uploadPart = uploadPartResponse.Data;
            }
            else
            {
                if(ErrorHelper.IsRecoverableError(uploadPartResponse.Error))
                {
                    if(retryCount > retryTimeoutCount)
                    {
                        uploadPartResponse.Error.Message = $"Hit retry max on error: {uploadPartResponse.Error.Message}";

                        return new BackblazeApiResponse<UploadedPartOutputDTO>()
                        {
                            Succeeded = false,
                            Error = uploadPartResponse.Error
                        };
                    }

                    retryCount++;
                    goto Retry;
                }
                else
                {
                    return new BackblazeApiResponse<UploadedPartOutputDTO>()
                    {
                        Succeeded = false,
                        Error = uploadPartResponse.Error
                    };
                }
            }

            return new BackblazeApiResponse<UploadedPartOutputDTO>()
            {
                Succeeded = uploadUrlResponse.Succeeded,
                Data = uploadPart
            };
        }

        public async Task<BackblazeApiResponse<FileVersionDeletedOutputDTO>> DeleteFileVersionAsync(
            AuthorizationOutputDTO auth,
            string fileName,
            string fileId,
            bool? bypassGovernance = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var body = SerializeToJsonContent(new DeleteFileVersionInputDTO()
            {
                FileName = fileName,
                FileId = fileId,
                BypassGovernance = bypassGovernance,
            });

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{auth.ApiUrl}/b2api/v2/b2_delete_file_version"),
                Content = body
            };

            request.Headers.TryAddWithoutValidation("Authorization", auth.AuthorizationToken);

            var response = await _client.SendAsync(request, cancellationToken);
            string responseJson = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
            {
                return new BackblazeApiResponse<FileVersionDeletedOutputDTO>()
                {
                    Succeeded = response.IsSuccessStatusCode,
                    Error = JsonSerializer.Deserialize<B2ErrorResponseOutputDTO>(responseJson)
                };
            }

            return new BackblazeApiResponse<FileVersionDeletedOutputDTO>()
            {
                Succeeded = response.IsSuccessStatusCode,
                Data = JsonSerializer.Deserialize<FileVersionDeletedOutputDTO>(responseJson)
            };
        }

        public async Task<BackblazeApiResponse<ListFileVersionsOutputDTO>> ListFileVersionsAsync(
            AuthorizationOutputDTO auth,
            string bucketId,
            string startFileName = null,
            string startFileId = null,
            int? maxFileCount = null,
            string prefix = null,
            string delimiter = null,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var body = SerializeToJsonContent(new ListFileVersionsInputDTO()
            {
                BucketId = bucketId,
                StartFileName = startFileName,
                StartFileId = startFileId,
                MaxFileCount = maxFileCount,
                Prefix = prefix,
                Delimiter = delimiter,
            });

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{auth.ApiUrl}/b2api/v2/b2_list_file_versions"),
                Content = body
            };

            request.Headers.TryAddWithoutValidation("Authorization", auth.AuthorizationToken);

            var response = await _client.SendAsync(request, cancellationToken);
            string responseJson = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
            {
                return new BackblazeApiResponse<ListFileVersionsOutputDTO>()
                {
                    Succeeded = response.IsSuccessStatusCode,
                    Error = JsonSerializer.Deserialize<B2ErrorResponseOutputDTO>(responseJson)
                };
            }

            return new BackblazeApiResponse<ListFileVersionsOutputDTO>()
            {
                Succeeded = response.IsSuccessStatusCode,
                Data = JsonSerializer.Deserialize<ListFileVersionsOutputDTO>(responseJson)
            };
        }
    }
}