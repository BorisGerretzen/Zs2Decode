using System.Xml.Serialization;
using Zs2Decode;

namespace Demo
{
    public static class XmlDecode
    {
        public static void Main() {
            // Files
            var inputFile = "./input.zs2";
            var outputFile = "./output.xml";

            // Get data
            Console.WriteLine("Decoding data...");
            var reader = new Zs2Decoder(inputFile);
            var rootChunk = reader.ReadData();

            // Write data to xml
            Console.WriteLine("Writing to xml...");
            var writer = new XmlSerializer(typeof(Chunk));
            var file = File.OpenWrite(outputFile);
            writer.Serialize(file, rootChunk as Chunk);
            file.Close();
            Console.WriteLine("Done");

        }
    }
}
