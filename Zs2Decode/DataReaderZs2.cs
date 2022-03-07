using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO.Compression;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Zs2Decode {
    public class DataReaderZs2 {
        private readonly string FileName;

        public DataReaderZs2(string fileName) {
            FileName = fileName;
        }

        private MemoryStream GetStream() {
            using FileStream stream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            using var gzip = new GZipStream(stream, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return output;
        }

        public void ReadData() {
            var bytes = GetStream().ToArray();
        }
    }
}