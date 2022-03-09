namespace Zs2Decode; 

public class Sensor {
    public readonly int Id;
    public readonly string Name;
    public readonly List<List<string>> Values;

    public Sensor(int id, string name) {
        Id = id;
        Name = name;
        Values = new List<List<string>>();
    }

    /// <summary>
    /// Adds the values of a new run to this sensor.
    /// </summary>
    /// <param name="values">The values of the run.</param>
    internal void AddValues(List<string> values) {
        Values.Add(values);
    }
}