using System.Xml.Serialization;
using Zs2Decode;

var reader = new DataReaderZs2("./test.zs2");
var chunk = reader.ReadData();
var writer =
    new XmlSerializer(typeof(Chunk));

var path = "./test_self.xml";
var file = File.OpenWrite(path);
writer.Serialize(file, chunk);
file.Close();