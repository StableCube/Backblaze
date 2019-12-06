
using System;
using System.Collections.Generic;

namespace StableCube.Backblaze.DotNetClient
{
    public struct TransferProgress
    {
        private static Object _lock = new Object();

        public long TotalBytes 
        { 
            get {
                lock(_lock)
                {
                    long value = 0;
                    foreach (var item in fileProgress)
                        value += item.Value.totalBytes;

                    return value;
                }
            }
        }

        public long BytesTransferred 
        { 
            get {
                lock(_lock)
                {
                    long value = 0;
                    foreach (var item in fileProgress)
                        value += item.Value.bytesTransferred;

                    return value;
                }
            }
        }

        public readonly IDictionary<string, FileProgress> fileProgress;

        public TransferProgress(IDictionary<string, FileProgress> fileProgress)
        {
            this.fileProgress = fileProgress;
        }
    }
}