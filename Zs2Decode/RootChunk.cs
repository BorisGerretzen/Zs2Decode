using System.Xml.Serialization;

namespace Zs2Decode; 

public class RootChunk : Chunk {
    [XmlIgnore] public readonly List<Sensor> Sensors;

    public RootChunk(string name, int type, string value) : base(name, type, value, 0) {
        Sensors = new List<Sensor>();
    }

    internal RootChunk() { }

    /// <summary>
    ///     Populates the Sensors list.
    /// </summary>
    internal void PopulateSensors() {
        // Get the names and IDs of the sensors first
        var channelManager = Navigate("Body/batch/SeriesDef/TestTaskDefs/Elem0/ChannelManager/ChannelManager");
        var sensorMap = new Dictionary<int, Sensor>();
        foreach (var child in channelManager.ListElements) {
            var id = int.Parse(child.Navigate("ID").Value);
            var name = child.Navigate("Name/Text").Value;
            var sensor = new Sensor(id, name);
            Sensors.Add(sensor);
            sensorMap.Add(id, sensor);
        }

        // Now get the each run
        var seriesChunk = Navigate("Body/batch/Series/SeriesElements");
        var i = 0;
        foreach (var runChunk in seriesChunk.ListElements) {
            // Get the values for each sensor in this run
            var runSensor = runChunk.Navigate("SeriesElements/Elem0/RealTimeCapture/Trs/SingleGroupDataBlock/DataChannels");
            foreach (var child in runSensor.ListElements) {
                var id = int.Parse(child.Navigate("TrsChannelId").Value);
                var data = child.Navigate("DataArray").Value;
                data = data.Replace("[", "");
                data = data.Replace("]", "");

                sensorMap[id].AddValues(data.Split(", ").ToList());
            }

            i++;
        }

        // Get all sensors that have values
        var sensors = Sensors.Where(sensor => sensor.Values.Count > 0).ToList();

        // Clear sensors and add only those that have values
        Sensors.Clear();
        Sensors.AddRange(sensors);
    }
}