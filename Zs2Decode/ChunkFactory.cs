using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Zs2Decode
{
    public class ChunkFactory {
        private Queue<byte> data;

        public ChunkFactory(byte[] bytes) {
            data = new Queue<byte>(bytes);
        }

        public List<Chunk> GenerateChunks() {
            var chunks = new List<Chunk>();
            int i = 0;
            while (data.Count > 5) {
                Console.WriteLine(i);
                chunks.Add(GetSingleFrombytes());
                i++;
            }

            return chunks;
        }

        /// <summary>
        /// Checks if bit 31 is set in a 4 byte array.
        /// If bit 31 is set to 1, it means the value is a string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True if bit 31 is set to 1.</returns>
        private bool Bit31Set(uint val) {
            return (val & (1<<31)) != 0;
        }

        /// <summary>
        /// Gets the string value from an AA chunk.
        /// </summary>
        /// <returns>The string contained in the chunk.</returns>
        /// <exception cref="Exception">Throws exception if bit 31 is not set.</exception>
        private string GetValueAA() {
            // Get the length of the target
            uint stringLength = BitConverter.ToUInt32(data.DequeueChunk(4).ToArray());

            // If bit 31 is set, the target is a string
            if (Bit31Set(stringLength)) {
                // Subtract 2^31
                stringLength -= 2147483648;

                // Get bytes and return string
                var stringBytes = data.DequeueChunk((int)stringLength*2).ToArray();
                stringBytes = stringBytes.Where(b => b != 0x00).ToArray();
                return Encoding.ASCII.GetString(stringBytes);
            }

            // I dont know if an AA type happens without bit 31 being set, here just in case.
            throw new Exception("Bit 31 not set");
        }

        /// <summary>
        /// Creates a chunk from an input array of bytes.
        /// Only pass in the 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public Chunk GetSingleFrombytes() {
            // Get name
            int nameLength = data.Dequeue();

            // We hit end of some element, this should do something when i start working on nesting.
            if (nameLength == 0xFF) {
                nameLength = data.Dequeue();
            }
            byte[] nameBytes = new byte[nameLength];
            for (int i = 0; i < nameLength; i++) {
                nameBytes[i] = data.Dequeue();
            }
            string name = Encoding.ASCII.GetString(nameBytes);

            // Get type
            int type = data.Dequeue();

            // Get value depending on type
            string val;
            switch (type) {
                // contains other elements
                case 0xDD:
                    val = "";
                    data.Dequeue();
                    break;
                // 2 byte int
                case 0x66:
                    val = BitConverter.ToInt16(data.DequeueChunk(2).ToArray()).ToString();
                    break;
                // 4 byte int
                case 0x11:
                    val = BitConverter.ToInt32(data.DequeueChunk(4).ToArray()).ToString();
                    break;
                // 4 byte uint
                case 0x44:
                    val = BitConverter.ToUInt32(data.DequeueChunk(4).ToArray()).ToString();
                    break;
                // string
                case 0xAA:
                    val = GetValueAA();
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }

            return new Chunk(name, type.ToString(), val);
        }
    }
}
