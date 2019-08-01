using System;
using System.IO;
using System.Collections.Generic;

namespace StableCube.Backblaze.DotNetClient
{
    public static class FileSplitter
    {
        public static List<SplitFilePart> SplitAll(string sourcePath, string outputDir, long partSize)
        {
            const long BUFFER_SIZE = 20 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];
            List<SplitFilePart> partPaths = new List<SplitFilePart>();

            using (FileStream input = System.IO.File.OpenRead(sourcePath))
            {
                int index = 0;
                while (input.Position < input.Length)
                {
                    string partPath = Path.Combine(outputDir, Guid.NewGuid().ToString());
                    using (FileStream output = System.IO.File.Create(partPath))
                    {
                        long remaining = partSize, bytesRead;

                        while (remaining > 0 && (bytesRead = input.Read(buffer, 0,
                                (int)Math.Min(remaining, BUFFER_SIZE))) > 0)
                        {
                            output.Write(buffer, 0, (int)bytesRead);
                            remaining -= bytesRead;
                        }

                        partPaths.Add(new SplitFilePart(
                            filePath: partPath,
                            partNumber: index + 1
                        ));

                        index++;
                    }
                }
            }

            return partPaths;
        }

        public static void Extract(string sourcePath, string outputPath, long offset, long count)
        {
            const long BUFFER_SIZE = 20 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];

            using (FileStream input = System.IO.File.OpenRead(sourcePath))
            {
                while (input.Position < input.Length)
                {
                    using (FileStream output = System.IO.File.Create(outputPath))
                    {
                        long remaining = count, bytesRead;

                        input.Seek(offset, SeekOrigin.Begin);

                        while (remaining > 0 && (bytesRead = input.Read(buffer, 0,
                                (int)Math.Min(remaining, BUFFER_SIZE))) > 0)
                        {
                            output.Write(buffer, 0, (int)bytesRead);
                            remaining -= bytesRead;
                        }

                        output.Close();
                        input.Close();

                        return;
                    }
                }
            }
        }
    }
}