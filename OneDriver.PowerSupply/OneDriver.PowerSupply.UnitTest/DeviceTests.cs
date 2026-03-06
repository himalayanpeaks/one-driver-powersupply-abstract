using Moq;
using OneDriver.Framework.Libs.Announcer;
using OneDriver.Framework.Libs.Validator;
using OneDriver.PowerSupply.Basic.Products;
using static OneDriver.Module.Definition;

namespace OneDriver.PowerSupply.Basic.UnitTest;

public class DeviceTests
{
    private readonly Mock<IValidator> _validatorMock;
    private readonly Mock<IPowerSupplyHal> _halMock;
    private readonly Device _device;
    private DataTunnel<InternalDataHal>.DataEventHandler? _processDataChangedCallback;

    public DeviceTests()
    {
        _validatorMock = new Mock<IValidator>();
        _halMock = new Mock<IPowerSupplyHal>();

        _halMock.Setup(h => h.NumberOfChannels).Returns(2);
        _halMock.Setup(h => h.MaxVoltageInVolts).Returns(30);
        _halMock.Setup(h => h.MaxCurrentInAmpere).Returns(5);
        _halMock.Setup(h => h.Mode).Returns(new OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode[2]);

        _halMock.Setup(h => h.AttachToProcessDataEvent(It.IsAny<DataTunnel<InternalDataHal>.DataEventHandler>()))
            .Callback<DataTunnel<InternalDataHal>.DataEventHandler>(callback => _processDataChangedCallback = callback);

        _device = new Device("TestDevice", _validatorMock.Object, _halMock.Object);

        // Force initialization of MaxAmps and MaxVolts by accessing them once
        _ = _device.Parameters.MaxAmps;
        _ = _device.Parameters.MaxVolts;
    }

    [Fact]
    public void Constructor_InitializesDeviceWithCorrectChannelCount()
    {
        Assert.Equal(2, _device.Elements.Count);
    }

    [Fact]
    public void AllChannelsOn_CallsHAL_AllOn()
    {
        _halMock.Setup(h => h.AllOn()).Returns(DeviceError.NoError);

        var result = _device.AllChannelsOn();

        _halMock.Verify(h => h.AllOn(), Times.Once);
        Assert.Equal((int)DeviceError.NoError, result);
    }

    [Fact]
    public void AllChannelsOff_CallsHAL_AllOff()
    {
        _halMock.Setup(h => h.AllOff()).Returns(DeviceError.NoError);

        var result = _device.AllChannelsOff();

        _halMock.Verify(h => h.AllOff(), Times.Once);
        Assert.Equal((int)DeviceError.NoError, result);
    }

    [Fact]
    public void SetVolts_CallsHAL_WithCorrectParameters()
    {
        int channel = 0;
        double volts = 12.5;
        _halMock.Setup(h => h.SetDesiredVolts(channel, volts)).Returns(DeviceError.NoError);

        var result = _device.SetVolts(channel, volts);

        _halMock.Verify(h => h.SetDesiredVolts(channel, volts), Times.Once);
        Assert.Equal((int)DeviceError.NoError, result);
    }

    [Fact]
    public void SetAmps_CallsHAL_WithCorrectParameters()
    {
        int channel = 1;
        double amps = 3.2;
        _halMock.Setup(h => h.SetDesiredAmps(channel, amps)).Returns(DeviceError.NoError);

        var result = _device.SetAmps(channel, amps);

        _halMock.Verify(h => h.SetDesiredAmps(channel, amps), Times.Once);
        Assert.Equal((int)DeviceError.NoError, result);
    }

    [Fact]
    public void ProcessDataChanged_UpdatesChannelProcessData()
    {
        var initialTimeStamp = _device.Elements[0].ProcessData.TimeStamp;

        var internalData = new InternalDataHal(0, 12.5, 2.3);

        _processDataChangedCallback?.Invoke(this, internalData);

        Assert.Equal(12.5, _device.Elements[0].ProcessData.Voltage);
        Assert.Equal(2.3, _device.Elements[0].ProcessData.Current);
        Assert.NotEqual(initialTimeStamp, _device.Elements[0].ProcessData.TimeStamp);
    }

    [Fact]
    public void ProcessDataChanged_UpdatesCorrectChannel()
    {
        var initialTimeStamp = _device.Elements[1].ProcessData.TimeStamp;
        var internalData = new InternalDataHal(1, 5.0, 1.5);

        _processDataChangedCallback?.Invoke(this, internalData);

        Assert.Equal(5.0, _device.Elements[1].ProcessData.Voltage);
        Assert.Equal(1.5, _device.Elements[1].ProcessData.Current);
        Assert.NotEqual(initialTimeStamp, _device.Elements[1].ProcessData.TimeStamp);
    }

    [Fact]
    public void ChannelParameters_DesiredAmpsChange_CallsHalSetDesiredAmps()
    {
        _halMock.Setup(h => h.SetDesiredAmps(0, It.IsAny<double>()))
            .Returns(DeviceError.NoError);

        _device.Elements[0].Parameters.DesiredAmps = 2.5;

        _halMock.Verify(h => h.SetDesiredAmps(0, 2.5), Times.Once);
    }

    [Fact]
    public void ChannelParameters_DesiredVoltsChange_CallsHalSetDesiredVolts()
    {
        _halMock.Setup(h => h.SetDesiredVolts(0, It.IsAny<double>()))
            .Returns(DeviceError.NoError);

        _device.Elements[0].Parameters.DesiredVolts = 15.0;

        _halMock.Verify(h => h.SetDesiredVolts(0, 15.0), Times.Once);
    }

    [Fact]
    public void ChannelParameters_DesiredAmpsExceedsMax_ValidationFails()
    {
        var initialValue = _device.Elements[0].Parameters.DesiredAmps;

        // Attempt to set value exceeding max
        _device.Elements[0].Parameters.DesiredAmps = 10.0; // Max is 5.0

        // Value should remain unchanged due to validation failure
        Assert.Equal(initialValue, _device.Elements[0].Parameters.DesiredAmps);
    }

    [Fact]
    public void ChannelParameters_DesiredVoltsExceedsMax_ValidationFails()
    {
        var initialValue = _device.Elements[0].Parameters.DesiredVolts;

        // Attempt to set value exceeding max
        _device.Elements[0].Parameters.DesiredVolts = 50.0; // Max is 30.0

        // Value should remain unchanged due to validation failure
        Assert.Equal(initialValue, _device.Elements[0].Parameters.DesiredVolts);
    }

    [Fact]
    public void DeviceParameters_MaxVolts_ReturnsHalMaxVoltage()
    {
        Assert.Equal(30, _device.Parameters.MaxVolts);
    }

    [Fact]
    public void DeviceParameters_MaxAmps_ReturnsHalMaxCurrent()
    {
        Assert.Equal(5, _device.Parameters.MaxAmps);
    }
}