
namespace StableCube.Backblaze.DotNetClient
{
    public struct FilePartProgress
    {
        public readonly string filename;

        public readonly int partNumber;

        public readonly long totalBytes;

        public readonly long bytesTransferred;

        public FilePartProgress(
            string filename,
            int partNumber,
            long totalBytes = 0,
            long bytesTransferred = 0
        )
        {
            this.filename = filename;
            this.partNumber = partNumber;
            this.totalBytes = totalBytes;
            this.bytesTransferred = bytesTransferred;
        }
    }
}