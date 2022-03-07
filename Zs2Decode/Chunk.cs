using System;
using System.Collections.Generic;
using System.Text;

namespace Zs2Decode
{
    public class Chunk {
        public string Name;
        public string Type;
        public string Value;
        public List<Chunk> Children;

        public Chunk(string name, string type, string value) {
            Name = name;
            Type = type;
            Value = value;
            Children = new List<Chunk>();
        }
    }
}
