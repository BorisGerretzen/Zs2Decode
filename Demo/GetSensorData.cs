using Zs2Decode;

namespace Demo;

public static class GetSensorData {
    public static void Main() {
        // File
        var inputFile = "./input.zs2";

        // Get data
        Console.WriteLine("Decoding data...");
        var reader = new Zs2Decoder(inputFile);
        var rootChunk = reader.ReadData();

        // Get sensors
        var sensors = rootChunk.Sensors;

        // Loop through each sensor
        foreach (var sensor in sensors) {
            Console.WriteLine(sensor.Name);

            // Loop from 0 to the max row of all runs
            for (int row = 0; row < sensor.Values.Max(runVals => runVals.Count); row++) {
                // Loop through each run
                for (int run = 0; run < sensor.Values.Count; run++) {
                    // Print value if it exists in this row
                    if (row < sensor.Values[run].Count) {
                        // Doesnt properly format but Ill fix this later
                        Console.Write($"{sensor.Values[run][row]}\t\t");
                    }
                }
                Console.Write("\n");
            }
        }

        Console.WriteLine("Done");
    }
}