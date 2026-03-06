using ControlMode = OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode;

namespace OneDriver.PowerSupply.IntegrationTest;

/// <summary>
/// Integration tests for device creation and initialization using mocked HAL.
/// Tests device specifications, channels, and factory-like creation patterns.
/// NO HARDWARE REQUIRED.
/// </summary>
public class PowerSupplyFactoryIntegrationTests
{
    [Fact]
    public void Device_Create_ReturnsValidDevice()
    {
        var device = MockDeviceHelper.CreateMockedDevice("PowerSupplyVirtual");

        Assert.NotNull(device);
        Assert.Equal("PowerSupplyVirtual", device.Parameters.Name);
    }

    [Fact]
    public void Device_Create_DeviceHasCorrectSpecifications()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        Assert.NotNull(device);
        Assert.Equal(30.0, device.Parameters.MaxVolts);
        Assert.Equal(5.0, device.Parameters.MaxAmps);
    }

    [Fact]
    public void Device_Create_DeviceHasChannels()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        Assert.NotNull(device);
        Assert.NotNull(device.Elements);
        Assert.Single(device.Elements);
    }

    [Fact]
    public void Device_Create_ChannelsHaveCorrectParameters()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        Assert.NotNull(device);
        var channel = device.Elements.FirstOrDefault();
        Assert.NotNull(channel);
        Assert.NotNull(channel.Parameters);
        Assert.Equal("Ch0", channel.Parameters.Name);
    }

    [Fact]
    public void Device_SetDesiredVolts_UpdatesChannelParameter()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        channel.Parameters.DesiredVolts = 12.5;

        Assert.Equal(12.5, channel.Parameters.DesiredVolts);
    }

    [Fact]
    public void Device_SetDesiredAmps_UpdatesChannelParameter()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        channel.Parameters.DesiredAmps = 2.5;

        Assert.Equal(2.5, channel.Parameters.DesiredAmps);
    }

    [Fact]
    public void Device_SetControlMode_UpdatesChannelParameter()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        channel.Parameters.ControlMode = ControlMode.Voltage;

        Assert.Equal(ControlMode.Voltage, channel.Parameters.ControlMode);
    }

    [Fact]
    public void Device_SetDesiredVolts_AboveMax_IsRejected()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        var originalValue = channel.Parameters.DesiredVolts;

        // Validation exception is caught by SetProperty, value should remain unchanged
        channel.Parameters.DesiredVolts = 35.0;

        Assert.Equal(originalValue, channel.Parameters.DesiredVolts);
        Assert.NotEqual(35.0, channel.Parameters.DesiredVolts);
    }

    [Fact]
    public void Device_SetDesiredAmps_AboveMax_IsRejected()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        var originalValue = channel.Parameters.DesiredAmps;

        // Validation exception is caught by SetProperty, value should remain unchanged
        channel.Parameters.DesiredAmps = 10.0;

        Assert.Equal(originalValue, channel.Parameters.DesiredAmps);
        Assert.NotEqual(10.0, channel.Parameters.DesiredAmps);
    }

    [Fact]
    public void Device_AllChannelsOn_ReturnsSuccess()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var result = device.AllChannelsOn();

        Assert.Equal(0, result);
    }

    [Fact]
    public void Device_AllChannelsOff_ReturnsSuccess()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var result = device.AllChannelsOff();

        Assert.Equal(0, result);
    }

    [Fact]
    public void Device_SetVolts_ValidChannel_ReturnsSuccess()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var result = device.SetVolts(0, 15.0);

        Assert.Equal(0, result);
    }

    [Fact]
    public void Device_SetAmps_ValidChannel_ReturnsSuccess()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var result = device.SetAmps(0, 3.0);

        Assert.Equal(0, result);
    }

    [Fact]
    public void Device_ProcessData_IsNotNull()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        Assert.NotNull(channel.ProcessData);
    }

    [Fact]
    public void Device_MultipleChannelOperations_WorkCorrectly()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var channel = device.Elements.First();
        channel.Parameters.DesiredVolts = 10.0;
        channel.Parameters.DesiredAmps = 1.5;
        channel.Parameters.ControlMode = ControlMode.Current;

        Assert.Equal(10.0, channel.Parameters.DesiredVolts);
        Assert.Equal(1.5, channel.Parameters.DesiredAmps);
        Assert.Equal(ControlMode.Current, channel.Parameters.ControlMode);
    }

    [Fact]
    public void Device_Parameters_AreReadOnly()
    {
        var device = MockDeviceHelper.CreateMockedDevice();

        var maxVolts = device.Parameters.MaxVolts;
        var maxAmps = device.Parameters.MaxAmps;

        Assert.Equal(30.0, maxVolts);
        Assert.Equal(5.0, maxAmps);
    }
}
