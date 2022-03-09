using System.Data;
using System.Text;

namespace Zs2Decode;

internal class ChunkFactory {
    private DataHolder data;

    public ChunkFactory(byte[] bytes) {
        data = new DataHolder(bytes);
    }

    /// <summary>
    ///     Generates the chunk structure from the stored data.
    /// </summary>
    /// <returns>The root node of the structure.</returns>
    public Chunk GenerateChunks() {
        // Get root chunk
        RootChunk rootChunk = (RootChunk)GetNextChunk(true);
        var currentChunk = (Chunk)rootChunk;
        List<Chunk> chunks = new List<Chunk>();
        while (data.Count > 0) {
            // 0xFF is a closing tag, set current node to parent.
            while (data.First != null && data.First.Value == 0xFF) {
                currentChunk = currentChunk.Parent;
                data.RemoveFirst();
            }

            // Only create new chunk if there is data
            if (data.First != null) {
                var newChunk = GetNextChunk();
                chunks.Add(newChunk);
                currentChunk.AddChild(newChunk);
                if (newChunk.Type == 0xDD) currentChunk = newChunk;
            }
        }

        rootChunk.PopulateSensors();
        return rootChunk;
    }

    /// <summary>
    ///     Checks if bit 31 is set in a 4 byte number.
    ///     If bit 31 is set to 1, it means the value is a string.
    /// </summary>
    /// <param name="val"></param>
    /// <returns>True if bit 31 is set to 1.</returns>
    private bool Bit31Set(uint val) {
        return (val & (1 << 31)) != 0;
    }

    /// <summary>
    ///     Gets the string value from an AA chunk.
    /// </summary>
    /// <returns>The string contained in the chunk.</returns>
    /// <exception cref="Exception">Throws exception if bit 31 is not set.</exception>
    private string GetValueAA() {
        // Get the length of the target
        var stringLength = BitConverter.ToUInt32(data.DequeueChunk(4).ToArray());

        // If bit 31 is set, the target is a string
        if (Bit31Set(stringLength)) {
            // Subtract 2^31
            stringLength -= 2147483648;

            // Get bytes and return string
            return data.GetString((int)stringLength, true);
        }

        // I dont know if an AA type happens without bit 31 being set, here just in case.
        throw new Exception("Bit 31 not set");
    }

    /// <summary>
    ///     Appends the given value and a comma to the stringbuilder.
    /// </summary>
    /// <param name="builder">The stringbuilder.</param>
    /// <param name="value">Value to be added.</param>
    private void AppendListStringBuilder(StringBuilder builder, object value) {
        builder.Append(value);
        builder.Append(", ");
    }

    private void CountStrings(LinkedList<byte> bytes) { }

    /// <summary>
    ///     Gets the string value of an EE chunk
    /// </summary>
    /// <returns>String representation of the value</returns>
    /// <exception cref="NotImplementedException">If list type is not implemented yet</exception>
    private string GetValueEE(string name) {
        var identificationBytes = data.DequeueChunk(2).ToArray();
        var length = data.GetInt32();
        var builder = new StringBuilder();
        builder.Append("[");

        // If identification bytes are 0x1100, it is a special list
        // https://zs2decode.readthedocs.io/en/latest/special_chunks.html
        // Currently not working, outputs hex data 
        if (identificationBytes.SequenceEqual(new byte[] { 0x11, 0x00 })) {
            if (name == "QS_TextPar") {
                AppendListStringBuilder(builder, data.Dequeue());
                AppendListStringBuilder(builder, GetValueAA());
                AppendListStringBuilder(builder, GetValueAA());
                AppendListStringBuilder(builder, GetValueAA());
                AppendListStringBuilder(builder, GetValueAA());
            }
            else if (name == "QS_ValPar") {
                data.CreateCheckpoint();
                AppendListStringBuilder(builder, data.Dequeue());
                AppendListStringBuilder(builder, data.GetDoubleFP());
                AppendListStringBuilder(builder, GetValueAA());
                AppendListStringBuilder(builder, data.GetInt16());
                for (int i = 0; i < 9; i++) {
                    AppendListStringBuilder(builder, data.Dequeue());
                }

                if (data.CheckpointCount != length) {
                    data.RestoreCheckPoint();
                    for (int i = 0; i < 9; i++) {
                        AppendListStringBuilder(builder, data.Dequeue());
                    }

                    AppendListStringBuilder(builder, GetValueAA());
                    for (int i = 0; i < 38; i++) {
                        AppendListStringBuilder(builder, data.Dequeue());
                    }
                }
            }
            else {
                builder.Append(BitConverter.ToString(data.DequeueChunk(length).ToArray()));
            }
        }
        // 0x04 => single precision floating point
        else if (identificationBytes.SequenceEqual(new byte[] { 0x04, 0x00 }))
            for (var i = 0; i < length; i++)
                AppendListStringBuilder(builder, data.GetSingleFP());
        // 0x05 => double precision floating point
        else if (identificationBytes.SequenceEqual(new byte[] { 0x05, 0x00 }))
            for (var i = 0; i < length; i++)
                AppendListStringBuilder(builder, data.GetDoubleFP());
        // 0x11 => bytes
        else if (identificationBytes.SequenceEqual(new byte[] { 0x11, 0x00 }))
            for (var i = 0; i < length; i++)
                AppendListStringBuilder(builder, data.Dequeue());
        // 0x16 => 32 bit integers.
        else if (identificationBytes.SequenceEqual(new byte[] { 0x16, 0x00 }))
            for (var i = 0; i < length; i++)
                AppendListStringBuilder(builder, data.GetInt32());
        // 0x00 => empty
        else if (identificationBytes.SequenceEqual(new byte[] { 0x00, 0x00 }))
            return "[]";
        else
            throw new NotImplementedException();

        // Remove final comma and return
        builder.Remove(builder.Length - 3, 2);
        builder.Append("]");
        return builder.ToString();
    }

    /// <summary>
    ///     Starts reading from the Queue and returns the next chunk as an object.
    /// </summary>
    /// <returns>The next chunk</returns>
    private Chunk GetNextChunk(bool root = false) {
        // Get name and type
        int nameLength = data.Dequeue();
        var name = data.GetString(nameLength);
        int type = data.Dequeue();

        // Get value depending on type
        string val;
        switch (type) {
            // contains other elements
            case 0xDD:
                int valueLength = data.Dequeue();
                val = data.GetString(valueLength);
                break;
            // 2 byte uint 
            case 0x66:
                val = data.GetUInt16().ToString();
                break;
            // 2 byte int
            case 0x55:
                val = data.GetInt16().ToString();
                break;
            // 4 byte int
            case 0x11:
            case 0x33:
                val = data.GetInt32().ToString();
                break;
            // 4 byte uint
            case 0x44:
            case 0x22:
                val = data.GetUInt32().ToString();
                break;
            // string
            case 0xAA:
                val = GetValueAA();
                break;
            // Single precision floating point
            case 0xBB:
                val = data.GetSingleFP().ToString();
                break;
            // Double precision floating point
            case 0xCC:
                val = data.GetDoubleFP().ToString();
                break;
            // Byte
            case 0x88:
                val = data.Dequeue().ToString();
                break;
            // Boolean
            case 0x99:
                val = data.GetBool().ToString();
                break;
            // Lists
            case 0xEE:
                val = GetValueEE(name);
                break;
            default:
                throw new NotImplementedException();
        }

        if (root) {
            return new RootChunk(name, type, val);
        }

        return new Chunk(name, type, val, data.CurrentPosition);
    }
}