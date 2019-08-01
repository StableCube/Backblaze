
namespace StableCube.Backblaze.DotNetClient
{
    public struct UploadFile
    {
        public readonly string bucketId;

        public readonly string sourceFilePath;

        public readonly string destinationFilename;

        public UploadFile(
            string bucketId,
            string sourceFilePath,
            string destinationFilename
        )
        {
            this.bucketId = bucketId;
            this.sourceFilePath = sourceFilePath;
            this.destinationFilename = destinationFilename;
        }
    }
}