using System.Xml.Serialization;
using Zs2Decode;

// Files
var inputFile = "./input.zs2";
var outputFile = "./output.xml";

// Get data
var reader = new Zs2Decoder(inputFile);
var rootChunk = reader.ReadData();

// Write data to xml
var writer = new XmlSerializer(typeof(Chunk));
var file = File.OpenWrite(outputFile);
writer.Serialize(file, rootChunk);
file.Close();