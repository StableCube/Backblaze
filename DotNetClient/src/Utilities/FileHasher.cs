using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace StableCube.Backblaze.DotNetClient
{
    public static class FileHasher
    {
        public static string GetFileHash(byte[] fileData)
        {
            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] hash = sha1Hash.ComputeHash(fileData);

                return BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
            }
        }

        public static async Task<string> GetFileHashAsync(
            FileStream fileData, 
            long index, 
            long count,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            byte[] buffer = new byte[count];
            await fileData.ReadAsync(buffer, (int)index, (int)count, cancellationToken);

            return GetFileHash(buffer);
        }

        public static string GetFileHash(Stream fileStream)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(fileStream);

                return BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
            }
        }
    }
}