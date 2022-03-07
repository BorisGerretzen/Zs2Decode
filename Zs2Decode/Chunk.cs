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

    /// <summary>
    /// Constructor for xml serializer
    /// </summary>
    internal Chunk() {
        Children = new List<Chunk>();
    }

    /// <summary>
    /// Adds a child to the current chunk, also sets the child's parent to this node.
    /// </summary>
    /// <param name="child">The new child</param>
    public void AddChild(Chunk child) {
        Children.Add(child);
        child.Parent = this;
    }
}