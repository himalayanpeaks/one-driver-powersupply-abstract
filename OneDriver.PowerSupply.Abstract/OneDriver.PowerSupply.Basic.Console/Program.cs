// KD3005P Power Supply Test Program

using System.Runtime.CompilerServices;
using OneDriver.Toolbox;

Console.WriteLine("=== KD3005P Power Supply Test ===");
Console.WriteLine();

var powerSupply = OneDriver.PowerSupply.Factory.PowerSupplyFactory.Create(OneDriver.PowerSupply.Factory.PowerSupplyType.Kd3005p);

// Connect to the power supply
Console.WriteLine("Connecting to COM5...");
var result = powerSupply.Connect("COM5;9600");
if (result == 0)
{
    Console.WriteLine("✓ Connected successfully");
}
else
{
    Console.WriteLine($"✗ Connection failed with error code: {result}");
    return;
}

try
{
    // Display device info
    Console.WriteLine($"\nDevice Name: {powerSupply.Parameters.Name}");
    Console.WriteLine($"Max Voltage: {powerSupply.Parameters.MaxVolts}V");
    Console.WriteLine($"Max Current: {powerSupply.Parameters.MaxAmps}A");
    Console.WriteLine($"Number of Channels: {powerSupply.Elements.Count}");
    Console.WriteLine();

    // Test Channel 0
    Console.WriteLine("--- Testing Channel 0 ---");

    // Set voltage and current for channel 0
    Console.WriteLine("Setting Channel 0: 5.0V, 1.0A");
    powerSupply.SetVolts(0, 5.0);
    powerSupply.SetAmps(0, 1.0);
    Thread.Sleep(500);

    // Turn on channel
    Console.WriteLine("Turning all channels ON...");
    powerSupply.AllChannelsOn();
    Thread.Sleep(1000);

    int i = 0;
    // Read back process data
    while (i++ < 100)
    {
        Console.WriteLine($"Channel 0 Actual Voltage: {powerSupply.Elements[0].ProcessData.Voltage}V");
        Console.WriteLine($"Channel 0 Actual Current: {powerSupply.Elements[0].ProcessData.Current}A");
        Console.WriteLine();
        Tools.Wait(1000);
    }

    // Test different voltage levels
    Console.WriteLine("Testing different voltage levels...");
    double[] testVoltages = [3.3, 5.0, 12.0];

    foreach (var voltage in testVoltages)
    {
        Console.WriteLine($"\nSetting voltage to {voltage}V...");
        powerSupply.SetVolts(0, voltage);
        Thread.Sleep(1000);
        Console.WriteLine($"  Actual: {powerSupply.Elements[0].ProcessData.Voltage}V, {powerSupply.Elements[0].ProcessData.Current}A");
    }

    // Test current limiting
    Console.WriteLine("\nTesting current limit...");
    Console.WriteLine("Setting current limit to 0.5A");
    powerSupply.SetAmps(0, 0.5);
    Thread.Sleep(1000);
    Console.WriteLine($"  Actual: {powerSupply.Elements[0].ProcessData.Voltage}V, {powerSupply.Elements[0].ProcessData.Current}A");

    // Wait a bit before turning off
    Console.WriteLine("\nPress any key to turn off channels and disconnect...");
    Console.ReadKey();

    // Turn off all channels
    Console.WriteLine("\nTurning all channels OFF...");
    powerSupply.AllChannelsOff();
    Thread.Sleep(500);

    Console.WriteLine($"Channel 0 Final State: {powerSupply.Elements[0].ProcessData.Voltage}V, {powerSupply.Elements[0].ProcessData.Current}A");
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗ Error during testing: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
finally
{
    // Disconnect
    Console.WriteLine("\nDisconnecting...");
    result = powerSupply.Disconnect();
    if (result == 0)
    {
        Console.WriteLine("✓ Disconnected successfully");
    }
    else
    {
        Console.WriteLine($"✗ Disconnect failed with error code: {result}");
    }
}

Console.WriteLine("\n=== Test Complete ===");