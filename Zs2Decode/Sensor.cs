using System;
using System.Collections.Generic;
using System.Text;

namespace Zs2Decode
{
    public class Sensor {
        public readonly int Id;
        public readonly string Name;
        public readonly List<List<string>> Values;

        public Sensor(int id, string name) {
            Id = id;
            Name = name;
            Values = new List<List<string>>();
        }

        internal void AddValues(List<string> values) {
            Values.Add(values);
        }
    }
}
