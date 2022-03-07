namespace Zs2Decode; 

public class Chunk {
    public List<Chunk> Children;
    public string Name;
    public string Type;
    public string Value;

    public Chunk(string name, string type, string value) {
        Name = name;
        Type = type;
        Value = value;
        Children = new List<Chunk>();
    }
}