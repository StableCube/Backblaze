using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StableCube.Backblaze.DotNetClient
{
    public interface IB2ClientV2
    {

        Task<BackblazeApiResponse<AuthorizationOutputDTO>> AuthorizeAsync(
            string keyId, 
            string applicationKey,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<BackblazeApiResponse<UploadUrlOutputDTO>> GetUploadUrlAsync(
            AuthorizationOutputDTO auth, 
            string bucketId,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        /// <summary>
        /// Upload a file without any error recovery
        /// </summary>
        Task<BackblazeApiResponse<FileDataOutputDTO>> UploadSmallFileAsync(
            UploadUrlOutputDTO uploadUrl, 
            string sourcePath, 
            string destinationFilename,
            IProgress<FileProgress> progressData = null,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        /// <summary>
        /// Upload a file with attempted recovery from errors
        /// </summary>
        Task<BackblazeApiResponse<FileDataOutputDTO>> UploadSmallFileAsync(
            AuthorizationOutputDTO auth,
            string bucketId, 
            string sourcePath, 
            string destinationFilename,
            int retryTimeoutCount = 5,
            IProgress<FileProgress> progressData = null,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<BackblazeApiResponse<FileDataOutputDTO>> StartLargeFileAsync(
            AuthorizationOutputDTO auth, 
            string bucketId,
            string destinationFilename,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<BackblazeApiResponse<FileDataOutputDTO>> FinishLargeFileAsync(
            AuthorizationOutputDTO auth, 
            string fileId,
            IEnumerable<string> partHashes,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<BackblazeApiResponse<UploadPartUrlOutputDTO>> GetUploadPartUrlAsync(
            AuthorizationOutputDTO auth,
            FileDataOutputDTO uploadData,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<BackblazeApiResponse<UploadedPartOutputDTO>> UploadLargeFilePartAsync(
            AuthorizationOutputDTO auth,
            FileDataOutputDTO fileData,
            Stream partStream,
            int partNumber,
            int retryTimeoutCount = 5,
            IProgress<FilePartProgress> progressData = null,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<BackblazeApiResponse<FileVersionDeletedOutputDTO>> DeleteFileVersionAsync(
            AuthorizationOutputDTO auth,
            string fileName,
            string fileId,
            bool? bypassGovernance = null,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<BackblazeApiResponse<ListFileVersionsOutputDTO>> ListFileVersionsAsync(
            AuthorizationOutputDTO auth,
            string bucketId,
            string startFileName = null,
            string startFileId = null,
            int? maxFileCount = null,
            string prefix = null,
            string delimiter = null,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<BackblazeApiResponse<FileDataOutputDTO>> CopyFileAsync(
            AuthorizationOutputDTO auth,
            CopyFileInputDTO input,
            CancellationToken cancellationToken = default(CancellationToken)
        );
    }
}