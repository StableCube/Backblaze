using System.Text.Json.Serialization;

namespace StableCube.Backblaze.DotNetClient
{
    public class CopyFileInputDTO
    {
        /// <summary>
        /// The ID of the source file being copied. (required)
        /// </summary>
        [JsonPropertyName("sourceFileId")]
        public string SourceFileId { get; set; }

        /// <summary>
        /// The ID of the bucket where the copied file will be stored. 
        /// If this is not set, the copied file will be added to the same bucket as the source file.
        /// 
        /// Note that the bucket containing the source file and the destination bucket must belong to the same account. (optional)
        /// </summary>
        [JsonPropertyName("destinationBucketId")]
        public string DestinationBucketId { get; set; }

        /// <summary>
        /// The name of the new file being created. (required)
        /// </summary>
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// The range of bytes to copy. If not provided, the whole source file will be copied. (optional)
        /// </summary>
        [JsonPropertyName("range")]
        public string Range { get; set; }

        /// <summary>
        /// The strategy for how to populate metadata for the new file. 
        /// If COPY is the indicated strategy, then supplying the contentType or fileInfo param is an error. (optional)
        /// </summary>
        [JsonPropertyName("metadataDirective")]
        public string MetadataDirective { get; set; }

        /// <summary>
        /// Must only be supplied if the metadataDirective is REPLACE.
        /// The MIME type of the content of the file, which will be returned in the Content-Type header when downloading the file. 
        /// Use the Content-Type b2/x-auto to automatically set the stored Content-Type post upload. 
        /// In the case where a file extension is absent or the lookup fails, the Content-Type is set to application/octet-stream. (optional)
        /// </summary>
        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        /// <summary>
        /// Must only be supplied if the metadataDirective is REPLACE.
        /// This field stores the metadata that will be stored with the file. 
        /// It follows the same rules that are applied to b2_upload_file (optional)
        /// </summary>
        [JsonPropertyName("fileInfo")]
        public string FileInfo { get; set; }
    }
}