using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Zs2Decode {
    public class RootChunk : Chunk {
        [XmlIgnore]
        public readonly List<Sensor> Sensors;

        public RootChunk(string name, int type, string value) : base(name, type, value, 0) {
            Sensors = new List<Sensor>();
        }

        internal RootChunk() { }

        /// <summary>
        /// Populates the Sensors list.
        /// </summary>
        internal void PopulateSensors() {
            var channelManager = Navigate("Body/batch/SeriesDef/TestTaskDefs/Elem0/ChannelManager/ChannelManager");
            Dictionary<int, Sensor> sensorMap = new Dictionary<int, Sensor>();
            foreach (Chunk child in channelManager.ListElements) {
                var id = int.Parse(child.Navigate("ID").Value);
                var name = child.Navigate("Name/Text").Value;
                var sensor = new Sensor(id, name);
                Sensors.Add(sensor);
                sensorMap.Add(id, sensor);
            }

            var seriesChunk = Navigate("Body/batch/Series/SeriesElements");
            int i = 0;
            foreach (var runChunk in seriesChunk.ListElements) {
                var runSensor = runChunk.Navigate("SeriesElements/Elem0/RealTimeCapture/Trs/SingleGroupDataBlock/DataChannels");
                foreach (Chunk child in runSensor.ListElements)
                {
                    var id = int.Parse(child.Navigate("TrsChannelId").Value);
                    var data = child.Navigate("DataArray").Value;
                    data = data.Replace("[", "");
                    data = data.Replace("]", "");
                    sensorMap[id].AddValues(data.Split(", ").ToList());
                }
                i++;
            }
            var sensors = Sensors.Where(sensor => sensor.Values.Count > 0).ToList();
            Sensors.Clear();
            Sensors.AddRange(sensors);
        }
    }
}