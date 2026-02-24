using System.Collections.ObjectModel;
using Moq;
using OneDriver.Framework.Libs.Validator;
using OneDriver.Module;
using OneDriver.Module.Channel;
using OneDriver.Module.Device;
using OneDriver.PowerSupply.Abstract.Channels;

namespace OneDriver.PowerSupply.Abstract.UnitTest;

public class CommonDeviceTests
{
    private class TestDevice(
        CommonDeviceParams parameters,
        IValidator validator,
        ObservableCollection<BaseChannel<CommonChannelParams, CommonProcessData>> elements)
        : CommonDevice<CommonDeviceParams, CommonChannelParams, CommonProcessData>(parameters, validator, elements)
    {
        protected override string GetErrorMessageFromDerived(int code)
        {
            return $"Derived error: {code}";
        }

        protected override int OpenConnection(string initString) => 0;
        protected override int CloseConnection() => 0;
        public override int AllChannelsOff() => 0;
        public override int AllChannelsOn() => 0;
        public override int SetAmps(int channelNumber, double amps) => channelNumber >= 0 ? 0 : -1;
        public override int SetVolts(int channelNumber, double volts) => channelNumber >= 0 ? 0 : -1;
    }


    [Fact]
    public void Connect_ValidInitString_ConnectsSuccessfully()
    {
        var validator = new Mock<IValidator>();
        validator.Setup(v => v.Validate(It.IsAny<string>())).Returns(true);

        var deviceParams = new CommonDeviceParams("TestDevice");
        var channels = new ObservableCollection<BaseChannel<CommonChannelParams, CommonProcessData>>();
        var device = new TestDevice(deviceParams, validator.Object, channels);

        var result = device.Connect("COM3;19200");

        Assert.Equal((int)Definition.DeviceError.NoError, result);
    }

    [Fact]
    public void Connect_InvalidInitString_ReturnsInvalidInitStringError()
    {
        var validator = new Mock<IValidator>();
        validator.Setup(v => v.Validate(It.IsAny<string>())).Returns(false);
        validator.Setup(v => v.GetExample()).Returns("COM23;19200");

        var deviceParams = new CommonDeviceParams("TestDevice");
        var channels = new ObservableCollection<BaseChannel<CommonChannelParams, CommonProcessData>>();
        var device = new TestDevice(deviceParams, validator.Object, channels);

        var result = device.Connect("INVALID");

        Assert.Equal((int)Definition.DeviceError.InvalidInitString, result);
    }

    [Fact]
    public void Disconnect_WhenConnected_DisconnectsSuccessfully()
    {
        var validator = new Mock<IValidator>();
        validator.Setup(v => v.Validate(It.IsAny<string>())).Returns(true);

        var deviceParams = new CommonDeviceParams("TestDevice");
        var channels = new ObservableCollection<BaseChannel<CommonChannelParams, CommonProcessData>>();
        var device = new TestDevice(deviceParams, validator.Object, channels);

        device.Connect("COM3;19200");
        var result = device.Disconnect();

        Assert.Equal((int)Definition.DeviceError.NoError, result);
    }

    [Fact]
    public void SetVolts_ValidChannel_ReturnsZero()
    {
        var device = new TestDevice(new CommonDeviceParams("Test"), new Mock<IValidator>().Object,
            new ObservableCollection<BaseChannel<CommonChannelParams, CommonProcessData>>());

        var result = device.SetVolts(1, 5.0);

        Assert.Equal(0, result);
    }

    [Fact]
    public void SetAmps_InvalidChannel_ReturnsError()
    {
        var device = new TestDevice(new CommonDeviceParams("Test"), new Mock<IValidator>().Object,
            new ObservableCollection<BaseChannel<CommonChannelParams, CommonProcessData>>());

        var result = device.SetAmps(-1, 2.0);

        Assert.Equal(-1, result);
    }
}