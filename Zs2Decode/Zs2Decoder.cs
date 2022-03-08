﻿using System.IO.Compression;

namespace Zs2Decode;

public class Zs2Decoder {
    private readonly string FileName;

    public Zs2Decoder(string fileName) {
        FileName = fileName;
    }

    /// <summary>
    ///     Decompresses the zs2 and creates a MemoryStream of it.
    /// </summary>
    /// <returns>MemoryStream of the target file.</returns>
    private MemoryStream GetStream() {
        using var stream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
        using var gzip = new GZipStream(stream, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return output;
    }

    /// <summary>
    ///     Reads the data from the given filename and returns the root chunk of the file.
    /// </summary>
    /// <returns>The root chunk of the file.</returns>
    public Chunk ReadData() {
        var bytes = GetStream().ToArray();
        var fac = new ChunkFactory(bytes.Skip(4).ToArray());
        var output = fac.GenerateChunks();
        return output;
    }
}