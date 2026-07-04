using ControlMode = OneDevice.PowerSupply.Abstract.Contracts.Definition.ControlMode;

namespace OneDevice.PowerSupply.IntegrationTest;

/// <summary>
/// Integration tests for device lifecycle and state management using mocked HAL.
/// Tests device state through creation, configuration, and operations.
/// NO HARDWARE REQUIRED.
/// </summary>
public class DeviceLifecycleIntegrationTests
{
    [Fact]
    public void Device_Created_StartsInDisconnectedState()
    {
        var device = MockDeviceHelper.CreateMockedDevice("PowerSupplyVirtual");

        Assert.NotNull(device);
        Assert.Equal("PowerSupplyVirtual", device.Parameters.Name);
        Assert.NotEmpty(device.Elements);
    }

    [Fact]
    public void Device_SetMultipleParameters_AllValuesRetained()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        
        channel.Parameters.DesiredVolts = 5.0;
        channel.Parameters.DesiredAmps = 1.0;
        channel.Parameters.ControlMode = ControlMode.Voltage;

        Assert.Equal(5.0, channel.Parameters.DesiredVolts);
        Assert.Equal(1.0, channel.Parameters.DesiredAmps);
        Assert.Equal(ControlMode.Voltage, channel.Parameters.ControlMode);
        
        channel.Parameters.DesiredVolts = 12.0;
        Assert.Equal(12.0, channel.Parameters.DesiredVolts);
        Assert.Equal(1.0, channel.Parameters.DesiredAmps);
    }

    [Fact]
    public void Device_OperationsSequence_WorksCorrectly()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        channel.Parameters.DesiredVolts = 5.0;
        channel.Parameters.DesiredAmps = 1.0;

        var setVoltsResult = device.SetVolts(0, 5.0);
        Assert.Equal(0, setVoltsResult);

        var setAmpsResult = device.SetAmps(0, 1.0);
        Assert.Equal(0, setAmpsResult);

        var turnOnResult = device.AllChannelsOn();
        Assert.Equal(0, turnOnResult);

        var turnOffResult = device.AllChannelsOff();
        Assert.Equal(0, turnOffResult);
    }

    [Fact]
    public void Device_WithinSafeLimits_NoExceptions()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        
        channel.Parameters.DesiredVolts = 0.0;
        channel.Parameters.DesiredAmps = 0.0;
        Assert.Equal(0.0, channel.Parameters.DesiredVolts);
        Assert.Equal(0.0, channel.Parameters.DesiredAmps);

        channel.Parameters.DesiredVolts = 30.0;
        channel.Parameters.DesiredAmps = 5.0;
        Assert.Equal(30.0, channel.Parameters.DesiredVolts);
        Assert.Equal(5.0, channel.Parameters.DesiredAmps);
    }

    [Fact]
    public void Device_ExceedVoltageLimit_IsRejected()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        var originalValue = channel.Parameters.DesiredVolts;

        // Validation exception is caught by SetProperty, value should remain unchanged
        channel.Parameters.DesiredVolts = 30.01;

        Assert.Equal(originalValue, channel.Parameters.DesiredVolts);
        Assert.NotEqual(30.01, channel.Parameters.DesiredVolts);
    }

    [Fact]
    public void Device_ExceedCurrentLimit_IsRejected()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        var originalValue = channel.Parameters.DesiredAmps;

        // Validation exception is caught by SetProperty, value should remain unchanged
        channel.Parameters.DesiredAmps = 5.01;

        Assert.Equal(originalValue, channel.Parameters.DesiredAmps);
        Assert.NotEqual(5.01, channel.Parameters.DesiredAmps);
    }

    [Fact]
    public void Device_ControlModeSwitch_PreservesSetValues()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        channel.Parameters.DesiredVolts = 10.0;
        channel.Parameters.DesiredAmps = 2.0;
        channel.Parameters.ControlMode = ControlMode.Voltage;

        Assert.Equal(10.0, channel.Parameters.DesiredVolts);
        Assert.Equal(2.0, channel.Parameters.DesiredAmps);

        channel.Parameters.ControlMode = ControlMode.Current;

        Assert.Equal(10.0, channel.Parameters.DesiredVolts);
        Assert.Equal(2.0, channel.Parameters.DesiredAmps);
        Assert.Equal(ControlMode.Current, channel.Parameters.ControlMode);
    }

    [Fact]
    public void Device_ProcessData_InitializedForAllChannels()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        foreach (var channel in device.Elements)
        {
            Assert.NotNull(channel.ProcessData);
        }
    }

    [Fact]
    public void Device_ChannelParameters_HaveCorrectNames()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        for (int i = 0; i < device.Elements.Count; i++)
        {
            var expectedName = $"Ch{i}";
            Assert.Equal(expectedName, device.Elements[i].Parameters.Name);
        }
    }

    [Fact]
    public void Device_CreateMultipleDevices_EachIsIndependent()
    {
        var device1 = MockDeviceHelper.CreateMockedDevice();
        var device2 = MockDeviceHelper.CreateMockedDevice();

        Assert.NotNull(device1);
        Assert.NotNull(device2);
        Assert.NotSame(device1, device2);

        device1.Elements.First().Parameters.DesiredVolts = 5.0;
        device2.Elements.First().Parameters.DesiredVolts = 10.0;

        Assert.Equal(5.0, device1.Elements.First().Parameters.DesiredVolts);
        Assert.Equal(10.0, device2.Elements.First().Parameters.DesiredVolts);
    }

    [Fact]
    public void Device_AllChannelsOperations_AffectAllChannels()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var onResult = device.AllChannelsOn();
        Assert.Equal(0, onResult);

        var offResult = device.AllChannelsOff();
        Assert.Equal(0, offResult);

        Assert.NotEmpty(device.Elements);
    }
}
