using System.Text;

namespace Zs2Decode;

internal class DataHolder : LinkedList<byte> {
    private List<byte> _checkpoint;
    private bool _checkpointSet;
    public int CurrentPosition=4;

    public DataHolder(IEnumerable<byte> collection) : base(collection) { }
    public int CheckpointCount => _checkpoint.Count;

    /// <summary>
    /// Creates a new checkpoint, all values dequeued after calling this function will be saved in a private list.
    /// </summary>
    public void CreateCheckpoint() {
        _checkpointSet = true;
        _checkpoint = new List<byte>();
    }

    /// <summary>
    /// Restores all the saved values from the private list to the normal list.
    /// </summary>
    public void RestoreCheckPoint() {
        _checkpointSet = false;
        CurrentPosition -= _checkpoint.Count;
        for (var i = _checkpoint.Count - 1; i >= 0; i--) AddFirst(_checkpoint[i]);
    }

    /// <summary>
    /// Returns the first n bytes from the list and removes them.
    /// </summary>
    /// <param name="chunkSize">Number of bytes to get and remove</param>
    /// <returns>First n bytes</returns>
    /// <exception cref="IndexOutOfRangeException">If first == null</exception>
    public IEnumerable<byte> DequeueChunk(int chunkSize) {
        for (var i = 0; i < chunkSize && Count > 0; i++)
            if (First != null)
                yield return Dequeue();
            else
                throw new IndexOutOfRangeException();
    }

    /// <summary>
    /// Returns the first byte of the list and removes it.
    /// </summary>
    /// <returns>First byte of the list</returns>
    /// <exception cref="IndexOutOfRangeException">If first == null</exception>
    public byte Dequeue() {
        if (First != null) {
            var val = First.Value;
            if (_checkpointSet) _checkpoint.Add(val);

            CurrentPosition++;
            RemoveFirst();
            return val;
        }

        throw new IndexOutOfRangeException();
    }

    #region Data collection from stream

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

    #endregion
}