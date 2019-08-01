
using System;
using System.Collections.Generic;

namespace StableCube.Backblaze.DotNetClient
{
    public struct TransferProgress
    {
        public long TotalBytes 
        { 
            get {
                long value = 0;
                foreach (var item in fileProgress)
                    value += item.Value.totalBytes;

                return value;
            }
        }

        public long BytesTransferred 
        { 
            get {
                long value = 0;
                foreach (var item in fileProgress)
                    value += item.Value.bytesTransferred;

                return value;
            }
        }

        public readonly IDictionary<string, FileProgress> fileProgress;

        public TransferProgress(IDictionary<string, FileProgress> fileProgress)
        {
            this.fileProgress = fileProgress;
        }
    }
}