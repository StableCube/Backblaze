using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StableCube.Backblaze.DotNetClient
{
    /// <summary>
    /// Higher level convenience client for the api
    /// </summary>
    public class BackblazeUploader : IBackblazeUploader
    {
        public IB2ClientV2 B2Client { get; private set; }

        public BackblazeUploader(
            IB2ClientV2 client
        )
        {
            B2Client = client;
        }

        public async Task<BackblazeApiResponse<FileDataOutputDTO>> UploadDynamicAsync(
            AuthorizationOutputDTO auth,
            UploadFile file,
            IProgress<TransferProgress> progressData = null,
            int retryTimeoutCount = 5,
            int concurrentUploads = 4,
            string tempDir = "/tmp",
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            ConcurrentDictionary<string, FileProgress> fileProgress = new ConcurrentDictionary<string, FileProgress>();
            long fileSize = new FileInfo(file.sourceFilePath).Length;
            fileProgress.TryAdd(file.destinationFilename, new FileProgress(filename: file.destinationFilename, totalBytes: fileSize));

            Task<BackblazeApiResponse<FileDataOutputDTO>> uploadTask;
            if(fileSize > auth.RecommendedPartSize)
            {
                uploadTask = ProcessLargeUploadAsync(
                    auth: auth, 
                    file: file, 
                    fileProgress: fileProgress,
                    fileSize: fileSize,
                    progressData: progressData, 
                    retryTimeoutCount: retryTimeoutCount, 
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                uploadTask = ProcessSmallUploadAsync(
                    auth: auth, 
                    file: file, 
                    fileProgress: fileProgress, 
                    progressData: progressData, 
                    retryTimeoutCount: retryTimeoutCount, 
                    cancellationToken: cancellationToken
                );
            }

            return await uploadTask;
        }

        public async Task<BackblazeApiResponse<FileDataOutputDTO>[]> UploadDynamicBatchAsync(
            AuthorizationOutputDTO auth,
            IEnumerable<UploadFile> files,
            IProgress<TransferProgress> progressData = null,
            int retryTimeoutCount = 5,
            int concurrentUploads = 4,
            string tempDir = "/tmp",
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            FileDataOutputDTO[] result = new FileDataOutputDTO[files.Count()];
            Task<BackblazeApiResponse<FileDataOutputDTO>>[] uploadTasks = new Task<BackblazeApiResponse<FileDataOutputDTO>>[files.Count()];
            Dictionary<string, FileProgress> fileProgress = new Dictionary<string, FileProgress>();

            foreach (var file in files)
            {
                long fileSize = new FileInfo(file.sourceFilePath).Length;

                fileProgress.Add(file.destinationFilename, new FileProgress(filename: file.destinationFilename, totalBytes: fileSize));
            }
            
            int fileI = 0;
            foreach (var file in files)
            {
                long fileSize = new FileInfo(file.sourceFilePath).Length;

                if(fileSize > auth.RecommendedPartSize)
                {
                    uploadTasks[fileI] = ProcessLargeUploadAsync(
                        auth: auth,
                        file: file,
                        fileProgress: fileProgress,
                        fileSize: fileSize,
                        progressData: progressData,
                        retryTimeoutCount: retryTimeoutCount,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    uploadTasks[fileI] = ProcessSmallUploadAsync(
                        auth: auth, 
                        file: file, 
                        fileProgress: fileProgress, 
                        progressData: progressData, 
                        retryTimeoutCount: retryTimeoutCount, 
                        cancellationToken: cancellationToken
                    );
                }

                fileI++;
            }

            SemaphoreSlim throttler = new SemaphoreSlim(concurrentUploads, concurrentUploads);
            IEnumerable<Task<BackblazeApiResponse<FileDataOutputDTO>>> tasks = uploadTasks.Select(async input =>
            {
                await throttler.WaitAsync();
                try
                {
                    return await input;
                }
                finally
                {
                    throttler.Release();
                }
            });

            return await Task.WhenAll(tasks);
        }

        private async Task<BackblazeApiResponse<FileDataOutputDTO>> ProcessLargeUploadAsync(
            AuthorizationOutputDTO auth,
            UploadFile file,
            IDictionary<string, FileProgress> fileProgress,
            long fileSize,
            IProgress<TransferProgress> progressData = null,
            int retryTimeoutCount = 5,
            string tempDir = "/tmp",
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            int partCount = (int)Math.Ceiling((double)fileSize / (double)auth.RecommendedPartSize);
            var initDataResponse = await B2Client.StartLargeFileAsync(auth, file.bucketId, file.destinationFilename, cancellationToken);
            if(!initDataResponse.Succeeded)
            {
                return new BackblazeApiResponse<FileDataOutputDTO>()
                {
                    Succeeded = false,
                    Error = initDataResponse.Error
                };
            }
            
            string[] partHashes = new string[partCount];
            List<UploadedPartOutputDTO> completeParts = new List<UploadedPartOutputDTO>();

            for (int i = 0; i < partCount; i++)
            {
                int partNumber = i + 1;
                long partSize = auth.RecommendedPartSize;
                if(partNumber == partCount)
                    partSize = fileSize - ((partCount - 1) * auth.RecommendedPartSize);

                long offset = i * auth.RecommendedPartSize;

                string tmpFilePath = Path.Combine(tempDir, Guid.NewGuid().ToString());
                FileSplitter.Extract(file.sourceFilePath, tmpFilePath, offset, partSize);

                using (var stream = System.IO.File.OpenRead(tmpFilePath))
                {
                    var uploadPartResponse = await B2Client.UploadLargeFilePartAsync(
                        auth: auth,
                        fileData: initDataResponse.Data,
                        partStream: stream,
                        partNumber: partNumber,
                        retryTimeoutCount: retryTimeoutCount,
                        cancellationToken: cancellationToken,
                        progressData: new Progress<FilePartProgress>((FilePartProgress progress) => {
                            var oldProgress = fileProgress[progress.filename];

                            long completeBytes = 0;
                            foreach (var completePart in completeParts)
                            {
                                completeBytes += completePart.ContentLength;
                            }

                            var newProgress = new FileProgress(
                                filename: progress.filename,
                                totalBytes: oldProgress.totalBytes,
                                bytesTransferred: completeBytes + progress.bytesTransferred
                            );

                            fileProgress[progress.filename] = newProgress;

                            progressData?.Report(new TransferProgress(
                                fileProgress: new Dictionary<string, FileProgress>(fileProgress)
                            ));
                        })
                    );

                    stream.Close();

                    if(!uploadPartResponse.Succeeded)
                    {
                        throw new B2Exception(uploadPartResponse.Error);
                    }
                    else
                    {
                        completeParts.Add(uploadPartResponse.Data);
                        partHashes[i] = uploadPartResponse.Data.ContentSha1;
                    }
                }

                File.Delete(tmpFilePath);
            }

            return await B2Client.FinishLargeFileAsync(auth, initDataResponse.Data.FileId, partHashes, cancellationToken);
        }

        private async Task<BackblazeApiResponse<FileDataOutputDTO>> ProcessSmallUploadAsync(
            AuthorizationOutputDTO auth,
            UploadFile file,
            IDictionary<string, FileProgress> fileProgress,
            IProgress<TransferProgress> progressData = null,
            int retryTimeoutCount = 5,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var result = await B2Client.UploadSmallFileAsync(
                auth: auth,
                bucketId: file.bucketId,
                sourcePath: file.sourceFilePath,
                destinationFilename: file.destinationFilename,
                retryTimeoutCount: retryTimeoutCount,
                cancellationToken: cancellationToken,
                progressData: new Progress<FileProgress>((FileProgress progress) => {
                    fileProgress[progress.filename] = progress;

                    progressData?.Report(new TransferProgress(
                        fileProgress: new Dictionary<string, FileProgress>(fileProgress)
                    ));
                })
            );

            return result;
        }
    }
}