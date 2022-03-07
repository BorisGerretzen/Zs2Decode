# Zs2Decode
This is an implementation of a decoder for ZwickRoell .zs2 files built in C#. Not everything is working yet, I skipped all of the ZIMT stuff because it was too much work to implement and I don't really need it.
Currently it is only possible to convert the entire file to xml, I am working on a way to just export the measurements.\
I would not have been able to make this without [Chris Petrich](https://github.com/cpetrich)'s [great documentation](https://zs2decode.readthedocs.io/en/latest/), thanks Chris!

## How to use
### Export to xml
```
using System.Xml.Serialization;
using Zs2Decode;

// Files
var inputFile = "./input.zs2";
var outputFile = "./output.xml";

// Get data
var reader = new DataReaderZs2(inputFile);
var rootChunk = reader.ReadData();

// Write data to xml
var writer = new XmlSerializer(typeof(Chunk));
var file = File.OpenWrite(outputFile);
writer.serialize(file, rootChunk);
file.Close();
```
