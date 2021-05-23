using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StableCube.Backblaze.DotNetClient
{
    public interface IBackblazeUploader
    {
        IB2ClientV2 B2Client { get; }

        /// <summary>
        /// Probes the file size and automatically does a small or large file upload.
        /// 
        /// Retries are attempted on recoverable errors
        /// </summary>
        Task<BackblazeApiResponse<FileDataOutputDTO>> UploadDynamicAsync(
            AuthorizationOutputDTO auth,
            UploadFile file,
            IProgress<TransferProgress> progressData = null,
            int retryTimeoutCount = 5,
            int concurrentUploads = 4,
            string tempDir = "/tmp",
            CancellationToken cancellationToken = default(CancellationToken)
        );

        /// <summary>
        /// A dynamic upload on multiple files
        /// </summary>
        Task<BackblazeApiResponse<FileDataOutputDTO>[]> UploadDynamicBatchAsync(
            AuthorizationOutputDTO auth,
            IEnumerable<UploadFile> files,
            IProgress<TransferProgress> progressData = null,
            int retryTimeoutCount = 5,
            int concurrentUploads = 4,
            string tempDir = "/tmp",
            CancellationToken cancellationToken = default(CancellationToken)
        );
    }
}