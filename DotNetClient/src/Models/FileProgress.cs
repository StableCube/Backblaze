
namespace StableCube.Backblaze.DotNetClient
{
    public struct FileProgress
    {
        public readonly string filename;

        public readonly long totalBytes;

        public readonly long bytesTransferred;

        public FileProgress(
            string filename,
            long totalBytes = 0,
            long bytesTransferred = 0
        )
        {
            this.filename = filename;
            this.totalBytes = totalBytes;
            this.bytesTransferred = bytesTransferred;
        }

        public FileProgress(
            FileProgress source,
            long bytesTransferred = 0
        )
        {
            this.filename = source.filename;
            this.totalBytes = source.totalBytes;
            this.bytesTransferred = bytesTransferred;
        }
    }
}