using System.Text;

namespace Zs2Decode;

internal class DataHolder : LinkedList<byte> {
    private List<byte> _checkpoint;
    private bool _checkpointSet;
    public int CheckpointCount => _checkpoint.Count;
    public int CurrentPosition = 0;

    public DataHolder(IEnumerable<byte> collection) : base(collection) { }

    public void CreateCheckpoint() {
        _checkpointSet = true;
        _checkpoint = new List<byte>();
    }

    public void RestoreCheckPoint() {
        _checkpointSet = false;
        for (int i = _checkpoint.Count - 1; i >= 0; i--) {
            AddFirst(_checkpoint[i]);
        }
    }

    public uint GetUInt32() {
        return BitConverter.ToUInt32(DequeueChunk(4).ToArray());
    }

    public short GetInt16() {
        return BitConverter.ToInt16(DequeueChunk(2).ToArray());
    }

    public ushort GetUInt16() {
        return BitConverter.ToUInt16(DequeueChunk(2).ToArray());
    }

    public int GetInt32() {
        return BitConverter.ToInt32(DequeueChunk(4).ToArray());
    }

    public float GetSingleFP() {
        return BitConverter.ToSingle(DequeueChunk(4).ToArray());
    }

    public double GetDoubleFP() {
        return BitConverter.ToDouble(DequeueChunk(8).ToArray());
    }

    public bool GetBool() {
        return Dequeue() == 0x01;
    }

    public long GetInt64() {
        return BitConverter.ToInt64(DequeueChunk(8).ToArray());
    }

    public string GetString(int len, bool remove0x00 = false) {
        var mult = remove0x00 ? 2 : 1;
        var bytes = DequeueChunk(len * mult).Where(b => b != 0x00).ToArray();
        return Encoding.ASCII.GetString(bytes);
    }

    public IEnumerable<byte> DequeueChunk(int chunkSize) {
        for (var i = 0; i < chunkSize && Count > 0; i++)
            if (First != null) {
                yield return Dequeue();
            }
            else {
                throw new IndexOutOfRangeException();
            }
    }

    public byte Dequeue() {
        if (First != null) {
            var val = First.Value;
            if (_checkpointSet) {
                _checkpoint.Add(val);
            }

            CurrentPosition++;
            RemoveFirst();
            return val;
        }

        throw new IndexOutOfRangeException();
    }
}