using OneDevice.PowerSupply.Abstract;
using OneDevice.PowerSupply.Abstract.Channels;
using OneDevice.Module.Channel;
using ControlMode = OneDevice.PowerSupply.Abstract.Contracts.Definition.ControlMode;

namespace OneDevice.PowerSupply.IntegrationTest;

/// <summary>
/// Integration tests for cross-component integration using mocked HAL.
/// Tests the integration between Device, Channels, and Parameters.
/// NO HARDWARE REQUIRED.
/// </summary>
public class CrossComponentIntegrationTests
{
    [Fact]
    public void Device_Channel_Integration_WorksTogether()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        Assert.NotNull(channel);
        Assert.NotNull(channel.Parameters);
        Assert.NotNull(channel.ProcessData);

        channel.Parameters.DesiredVolts = 15.0;
        var result = device.SetVolts(0, 15.0);

        Assert.Equal(0, result);
        Assert.Equal(15.0, channel.Parameters.DesiredVolts);
    }

    [Fact]
    public void Device_InheritsFromCommonDevice()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        Assert.IsAssignableFrom<CommonDevice<CommonDeviceParams, CommonChannelParams, CommonProcessData>>(device);
    }

    [Fact]
    public void Channel_IsBaseChannelType()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        Assert.IsAssignableFrom<BaseChannel<CommonChannelParams, CommonProcessData>>(channel);
    }

    [Fact]
    public void DeviceParams_ExposesMaxSpecifications()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var maxVolts = device.Parameters.MaxVolts;
        var maxAmps = device.Parameters.MaxAmps;

        Assert.True(maxVolts > 0);
        Assert.True(maxAmps > 0);
        Assert.Equal(30.0, maxVolts);
        Assert.Equal(5.0, maxAmps);
    }

    [Fact]
    public void Device_TypedAsCommonDevice_ProvidesAllFunctionality()
    {
        CommonDevice<CommonDeviceParams, CommonChannelParams, CommonProcessData> device = 
            MockDeviceHelper.CreateMockedDevice();

        Assert.NotNull(device);

        var result1 = device.SetVolts(0, 10.0);
        var result2 = device.SetAmps(0, 2.0);
        var result3 = device.AllChannelsOn();
        var result4 = device.AllChannelsOff();

        Assert.Equal(0, result1);
        Assert.Equal(0, result2);
        Assert.Equal(0, result3);
        Assert.Equal(0, result4);
    }

    [Fact]
    public void Device_ChannelCollection_IsObservable()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        Assert.NotNull(device.Elements);
        Assert.IsType<System.Collections.ObjectModel.ObservableCollection<BaseChannel<CommonChannelParams, CommonProcessData>>>(device.Elements);
    }

    [Fact]
    public void Device_ParameterValidation_PreventsInvalidStates()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();

        // Validation exception is caught by SetProperty, value should remain unchanged
        var originalValue = channel.Parameters.DesiredVolts;
        channel.Parameters.DesiredVolts = -1.0;

        Assert.Equal(originalValue, channel.Parameters.DesiredVolts);
        Assert.NotEqual(-1.0, channel.Parameters.DesiredVolts);
    }

    [Fact]
    public void Device_BoundaryValues_AtMaximum()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        
        channel.Parameters.DesiredVolts = device.Parameters.MaxVolts;
        channel.Parameters.DesiredAmps = device.Parameters.MaxAmps;

        Assert.Equal(device.Parameters.MaxVolts, channel.Parameters.DesiredVolts);
        Assert.Equal(device.Parameters.MaxAmps, channel.Parameters.DesiredAmps);
    }

    [Fact]
    public void Device_BoundaryValues_AtMinimum()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        
        channel.Parameters.DesiredVolts = 0.0;
        channel.Parameters.DesiredAmps = 0.0;

        Assert.Equal(0.0, channel.Parameters.DesiredVolts);
        Assert.Equal(0.0, channel.Parameters.DesiredAmps);
    }

    [Fact]
    public void Device_ControlModes_BothAvailable()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        
        channel.Parameters.ControlMode = ControlMode.Voltage;
        Assert.Equal(ControlMode.Voltage, channel.Parameters.ControlMode);

        channel.Parameters.ControlMode = ControlMode.Current;
        Assert.Equal(ControlMode.Current, channel.Parameters.ControlMode);
    }

    [Fact]
    public void Device_AllOperations_ReturnSuccessCode()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var results = new[]
        {
            device.SetVolts(0, 10.0),
            device.SetAmps(0, 2.0),
            device.AllChannelsOn(),
            device.AllChannelsOff()
        };

        Assert.All(results, result => Assert.Equal(0, result));
    }

    [Fact]
    public void Device_SequentialParameterChanges_AllApplied()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        
        for (double voltage = 1.0; voltage <= 5.0; voltage += 1.0)
        {
            channel.Parameters.DesiredVolts = voltage;
            Assert.Equal(voltage, channel.Parameters.DesiredVolts);
        }

        for (double current = 0.5; current <= 2.5; current += 0.5)
        {
            channel.Parameters.DesiredAmps = current;
            Assert.Equal(current, channel.Parameters.DesiredAmps);
        }
    }

    [Fact]
    public void Device_CreatesSpecificImplementation()
    {
        var device = MockDeviceHelper.CreateMockedDevice("PowerSupplyVirtual");

        Assert.Equal("PowerSupplyVirtual", device.Parameters.Name);
        Assert.Single(device.Elements);
        Assert.Equal(30.0, device.Parameters.MaxVolts);
        Assert.Equal(5.0, device.Parameters.MaxAmps);
    }
}
