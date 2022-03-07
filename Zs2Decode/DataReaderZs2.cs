using System.IO.Compression;

namespace Zs2Decode;

public class DataReaderZs2 {
    private readonly string FileName;

    public DataReaderZs2(string fileName) {
        FileName = fileName;
    }

    private MemoryStream GetStream() {
        using var stream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
        using var gzip = new GZipStream(stream, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return output;
    }

    public Chunk ReadData() {
        var bytes = GetStream().ToArray();
        var fac = new ChunkFactory(bytes.Skip(4).ToArray());
        var output = fac.GenerateChunks();
        return output;
    }
}