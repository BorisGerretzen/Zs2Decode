using System.Xml.Serialization;

namespace Zs2Decode;

[XmlRoot("chunk")]
[XmlInclude(typeof(RootChunk))]
public class Chunk {
    [XmlElement(typeof(Chunk), ElementName = "chunk")]
    public List<Chunk> Children;
    [XmlIgnore] public List<Chunk> ListElements => Children.Where(child => child.Name.StartsWith("Elem")).ToList();
    [XmlAttribute("name")] public string Name;
    [XmlIgnore] public Chunk Parent;
    [XmlAttribute("type")] public int Type;
    [XmlAttribute("value")] public string Value;
    [XmlIgnore] public int StartPosition;
    public Chunk(string name, int type, string value, int startPosition) {
        Name = name;
        Type = type;
        Value = value;
        StartPosition = startPosition;
        Children = new List<Chunk>();
    }

    /// <summary>
    ///     Constructor for xml serializer
    /// </summary>
    internal Chunk() {
        Children = new List<Chunk>();
    }

    /// <summary>
    ///     Adds a child to the current chunk, also sets the child's parent to this node.
    /// </summary>
    /// <param name="child">The new child</param>
    public void AddChild(Chunk child) {
        Children.Add(child);
        child.Parent = this;
    }

    /// <summary>
    /// Navigates through this Chunk's children.
    /// An example of a path: 'Header/ProgramVersionList/Elem0'. 
    /// </summary>
    /// <param name="path">Path with the name of the nodes you wish to navigate through.</param>
    /// <returns></returns>
    public Chunk Navigate(string path) {
        // If path is empty, we are the target node
        if (path == "" || path.Length == 1 && path[0] == '/') {
            return this;
        }

        // Remove slash in front of path cause we dont need it
        if (path[0] == '/') {
            path = path.Remove(0, 1);
        }

        // Capture child
        var splitPath = path.Split('/');
        var targetName = splitPath[0];
        var targetChunk = Children.First(child => child.Name == targetName);

        // Generate new path and return
        var nextPath = string.Join('/', splitPath[1..]);
        return targetChunk.Navigate(nextPath);
    }
}