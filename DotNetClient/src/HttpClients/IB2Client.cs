using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StableCube.Backblaze.DotNetClient
{
    public interface IB2Client
    {

        Task<Authorization> AuthorizeAsync(
            string keyId, 
            string applicationKey,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<UploadUrl> GetUploadUrlAsync(
            Authorization auth, 
            string bucketId,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        /// <summary>
        /// Upload a file without any error recovery
        /// </summary>
        Task<FileData> UploadSmallFileAsync(
            UploadUrl uploadUrl, 
            string sourcePath, 
            string destinationFilename,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        /// <summary>
        /// Upload a file with attempted recovery from errors
        /// </summary>
        Task<FileData> UploadSmallFileAsync(
            Authorization auth,
            string bucketId, 
            string sourcePath, 
            string destinationFilename,
            int retryTimeoutCount = 5,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<FileData> StartLargeFileUploadAsync(
            Authorization auth, 
            string bucketId,
            string destinationFilename,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<FileData> FinishLargeFileUploadAsync(
            Authorization auth, 
            string fileId,
            IEnumerable<string> partHashes,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<UploadPartUrl> GetUploadPartUrlAsync(
            Authorization auth,
            FileData uploadData,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<UploadedPart> UploadLargeFilePartAsync(
            UploadPartUrl uploadUrl, 
            string sourcePath,
            int partNumber,
            long maxPartSize,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<UploadedPart> UploadLargeFilePartAsync(
            Authorization auth,
            FileData fileData,
            string sourcePath,
            int partNumber,
            long maxPartSize,
            int retryTimeoutCount = 5,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        /// <summary>
        /// Probes the file size and automatically does a small or large file upload.
        /// 
        /// Retries are attempted on recoverable errors
        /// </summary>
        Task<FileData> UploadDynamicAsync(
            Authorization auth,
            string sourcePath,
            string bucketId,
            string destinationFilename,
            int retryTimeoutCount = 5,
            CancellationToken cancellationToken = default(CancellationToken)
        );
    }
}