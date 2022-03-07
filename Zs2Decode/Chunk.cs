using System.Xml.Serialization;

namespace Zs2Decode;

[XmlRoot("chunk")]
public class Chunk {
    [XmlElement(typeof(Chunk), ElementName = "chunk")]
    public List<Chunk> Children;

    [XmlAttribute("name")] public string Name;
    [XmlIgnore] public Chunk Parent;
    [XmlAttribute("type")] public int Type;
    [XmlAttribute("value")] public string Value;

    public Chunk(string name, int type, string value) {
        Name = name;
        Type = type;
        Value = value;
        Children = new List<Chunk>();
    }

    public Chunk() {
        Children = new List<Chunk>();
    }

    public void AddChild(Chunk child) {
        Children.Add(child);
        child.Parent = this;
    }

    public bool ShouldSerializeChildren() {
        return Children.Count > 0;
    }
}