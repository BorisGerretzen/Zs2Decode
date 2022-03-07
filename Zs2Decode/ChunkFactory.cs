using System.Text;

namespace Zs2Decode;

public class ChunkFactory {
    private readonly Queue<byte> data;

    public ChunkFactory(byte[] bytes) {
        data = new Queue<byte>(bytes);
    }

    public Chunk GenerateChunks() {
        // Get root chunk
        var rootChunk = GetSingleFrombytes();
        var currentChunk = rootChunk;
        var i = 0;
        while (data.Count > 0) {
            //Console.WriteLine(i);

            // If next is close
            byte peeked;
            while (data.TryPeek(out peeked) && peeked == 0xFF) {
                currentChunk = currentChunk.Parent;
                data.Dequeue();
            }

            if (data.TryPeek(out peeked)) {
                var newChunk = GetSingleFrombytes();
                currentChunk.AddChild(newChunk);
                if (newChunk.Type == 0xDD) currentChunk = newChunk;
            }

            i++;
        }

        return rootChunk;
    }

    /// <summary>
    ///     Checks if bit 31 is set in a 4 byte array.
    ///     If bit 31 is set to 1, it means the value is a string.
    /// </summary>
    /// <param name="data"></param>
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
            return GetString((int)stringLength, true);
        }

        // I dont know if an AA type happens without bit 31 being set, here just in case.
        throw new Exception("Bit 31 not set");
    }

    private void AppendListStringBuilder(StringBuilder builder, object value) {
        builder.Append(value);
        builder.Append(", ");
    }

    private string GetValueEE(string name) {
        var identificationBytes = data.DequeueChunk(2).ToArray();
        var length = GetInt32();

        var builder = new StringBuilder();
        builder.Append("[");

        // If identification bytes are 0x1100, it is a special list
        // https://zs2decode.readthedocs.io/en/latest/special_chunks.html

        if (identificationBytes.SequenceEqual(new byte[] { 0x11, 0x00 })) builder.Append(BitConverter.ToString(data.DequeueChunk(length).ToArray()));
        /*
            if (name == "QS_ParProp") {
                // 1 byte
                AppendListStringBuilder(builder, data.Dequeue());

                // 9 bools
                for (int i = 0; i < 9; i++) {
                    AppendListStringBuilder(builder, GetBool());
                }

                // 1 word
                AppendListStringBuilder(builder, GetInt16());

                // 9 strings
                // Seems like bit 31 is also set for these strings so we can use GetValueAA
                for (int i = 0; i < 9; i++) {
                    AppendListStringBuilder(builder, GetValueAA());
                }

                // 3 words
                AppendListStringBuilder(builder, GetInt16());
                AppendListStringBuilder(builder, GetInt16());
                AppendListStringBuilder(builder, GetInt16());

                // 5 strings
                for (int i = 0; i < 5; i++) {
                    AppendListStringBuilder(builder, GetValueAA());
                }

                // 1 long
                AppendListStringBuilder(builder, GetInt64());

                // 2 words
                // AppendListStringBuilder(builder, GetInt16());
                // AppendListStringBuilder(builder, GetInt16());

                // 1 byte
                AppendListStringBuilder(builder, data.Dequeue());

                // 1 string
                AppendListStringBuilder(builder, GetValueAA());

                // 4 booleans
                for (int i = 0; i < 5; i++) {
                    AppendListStringBuilder(builder, GetBool());
                }
            }
            else if (name == "QS_SkalProp") {
                AppendListStringBuilder(builder, data.Dequeue());
                AppendListStringBuilder(builder, GetValueAA());
                AppendListStringBuilder(builder, GetValueAA());

                AppendListStringBuilder(builder, GetBool());
                AppendListStringBuilder(builder, GetBool());

            }
            else {
                throw new NotImplementedException();
            }
            */
        else if (identificationBytes.SequenceEqual(new byte[] { 0x04, 0x00 }))
            for (var i = 0; i < length; i++)
                AppendListStringBuilder(builder, GetSingleFP());
        else if (identificationBytes.SequenceEqual(new byte[] { 0x05, 0x00 }))
            for (var i = 0; i < length; i++)
                AppendListStringBuilder(builder, GetDoubleFP());
        else if (identificationBytes.SequenceEqual(new byte[] { 0x11, 0x00 }))
            for (var i = 0; i < length; i++)
                AppendListStringBuilder(builder, data.Dequeue());
        else if (identificationBytes.SequenceEqual(new byte[] { 0x16, 0x00 }))
            for (var i = 0; i < length; i++)
                AppendListStringBuilder(builder, GetInt32());
        else if (identificationBytes.SequenceEqual(new byte[] { 0x00, 0x00 }))
            return "[]";
        else
            throw new NotImplementedException();

        builder.Remove(builder.Length - 3, 2);
        builder.Append("]");
        return builder.ToString();
    }

    /// <summary>
    ///     Creates a chunk from an input array of bytes.
    ///     Only pass in the
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public Chunk GetSingleFrombytes() {
        // Get name length
        int nameLength = data.Dequeue();

        // We hit end of some element, this should do something when i start working on nesting.
        while (nameLength == 0xFF) nameLength = data.Dequeue();

        // Get name
        var name = GetString(nameLength);

        // Get type
        int type = data.Dequeue();

        // Get value depending on type
        string val;
        switch (type) {
            // contains other elements
            case 0xDD:
                int valueLength = data.Dequeue();
                val = GetString(valueLength);
                break;
            // 2 byte int
            case 0x66:
            case 0x55:
                val = GetInt16().ToString();
                break;
            // 4 byte int
            case 0x11:
            case 0x33:
                val = GetInt32().ToString();
                break;
            // 4 byte uint
            case 0x44:
            case 0x22:
                val = GetUInt32().ToString();
                break;
            // string
            case 0xAA:
                val = GetValueAA();
                break;
            // Single precision floating point
            case 0xBB:
                val = GetSingleFP().ToString();
                break;
            // Double precision floating point
            case 0xCC:
                val = GetDoubleFP().ToString();
                break;
            // Byte
            case 0x88:
                val = data.Dequeue().ToString();
                break;
            // Boolean
            case 0x99:
                val = GetBool().ToString();
                break;
            case 0xEE:
                val = GetValueEE(name);
                break;
            default:
                throw new NotImplementedException();
                break;
        }

        return new Chunk(name, type, val);
    }

    #region Bytes to datatypes

    private uint GetUInt32() {
        return BitConverter.ToUInt32(data.DequeueChunk(4).ToArray());
    }

    private short GetInt16() {
        return BitConverter.ToInt16(data.DequeueChunk(2).ToArray());
    }

    private int GetInt32() {
        return BitConverter.ToInt32(data.DequeueChunk(4).ToArray());
    }

    private float GetSingleFP() {
        return BitConverter.ToSingle(data.DequeueChunk(4).ToArray());
    }

    private double GetDoubleFP() {
        return BitConverter.ToDouble(data.DequeueChunk(8).ToArray());
    }

    private bool GetBool() {
        return data.Dequeue() == 0x01;
    }

    private long GetInt64() {
        return BitConverter.ToInt64(data.DequeueChunk(8).ToArray());
    }

    private string GetString(int len, bool remove0x00 = false) {
        var mult = remove0x00 ? 2 : 1;
        var bytes = data.DequeueChunk(len * mult).Where(b => b != 0x00).ToArray();
        return Encoding.ASCII.GetString(bytes);
    }

    #endregion
}