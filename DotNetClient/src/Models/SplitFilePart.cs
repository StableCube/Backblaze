
namespace StableCube.Backblaze.DotNetClient
{
    public struct SplitFilePart
    {
        public readonly string filePath;

        public readonly int partNumber;

        public SplitFilePart(
            string filePath,
            int partNumber
        )
        {
            this.filePath = filePath;
            this.partNumber = partNumber;
        }
    }
}