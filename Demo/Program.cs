using Demo;

Menu();

void Menu() {
    while (true) {
        Console.Clear();
        Console.WriteLine("Select demo: ");
        Console.WriteLine("[1] - XML decode ");
        Console.WriteLine("[2] - Get sensor data ");
        Console.WriteLine("[3] - Quit");
        var choice = Console.ReadLine();

        if (choice == "1")
            XmlDecode.Main();
        else if (choice == "2")
            GetSensorData.Main();
        else if (choice == "3") Environment.Exit(0);
        Console.WriteLine("Press enter to continue");
        Console.ReadLine();
    }
}