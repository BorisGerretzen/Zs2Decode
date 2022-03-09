

# Zs2Decode
This is an implementation of a decoder for ZwickRoell .zs2 files built in C#. Currently the project supports converting .zs2 files to .xml, and getting the sensor data from a .zs2 file. As for decoding the .zs2 file, not all elements are correctly decoded, all elements starting with QS_, except QS_TextPar and QS_ValPar,  currently have the bytes from the binary as value.

I would not have been able to make this without [Chris Petrich](https://github.com/cpetrich)'s [great documentation](https://zs2decode.readthedocs.io/en/latest/), thanks Chris!



## How to install
To install using Nuget, run ```Install-Package Zs2Decode``` in the package manager console. The Nuget page can be found [here](https://www.nuget.org/packages/Zs2Decode/).

## How to use
These examples can also be found working in the Demo project included in the repository.
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
writer.Serialize(file, rootChunk);
file.Close();
```

### Get sensor data
```
using System.Xml.Serialization;
using Zs2Decode;

// Files
var inputFile = "./input.zs2";
var outputFile = "./output.xml";

// Get data
var reader = new DataReaderZs2(inputFile);
var rootChunk = reader.ReadData();

// Sensors are stored in Sensors variable
var sensors = rootChunk.Sensors;
```