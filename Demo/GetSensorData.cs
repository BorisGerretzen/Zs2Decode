using Zs2Decode;

namespace Demo; 

public static class GetSensorData {
    public static void Main() {
        // File
        var inputFile = "./input.zs2";

        // Get data
        Console.WriteLine("Decoding data...");
        var reader = new Zs2Decoder(inputFile);
        var rootChunk = reader.ReadData();

        // TODO finish demo

        Console.WriteLine("Done");
    }
}