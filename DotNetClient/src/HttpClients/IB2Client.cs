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
    }
}