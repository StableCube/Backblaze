using System;
using System.IO;
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
            IProgress<FileProgress> progressData = null,
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
            IProgress<FileProgress> progressData = null,
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
            Stream partStream,
            string destinationFilename,
            int partNumber,
            IProgress<FilePartProgress> progressData = null,
            CancellationToken cancellationToken = default(CancellationToken)
        );

        Task<UploadedPart> UploadLargeFilePartAsync(
            Authorization auth,
            FileData fileData,
            Stream partStream,
            int partNumber,
            int retryTimeoutCount = 5,
            IProgress<FilePartProgress> progressData = null,
            CancellationToken cancellationToken = default(CancellationToken)
        );
    }
}