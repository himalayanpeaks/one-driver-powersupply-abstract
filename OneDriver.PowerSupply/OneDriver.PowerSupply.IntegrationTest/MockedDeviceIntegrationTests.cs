using Moq;
using OneDriver.PowerSupply.Abstract;
using OneDriver.PowerSupply.Abstract.Channels;
using OneDriver.PowerSupply.Basic;
using OneDriver.PowerSupply.Basic.Products;
using OneDriver.Framework.Libs.Validator;

namespace OneDriver.PowerSupply.IntegrationTest;

/// <summary>
/// Integration tests using mocked HAL layer.
/// These tests verify the integration between Device, Channels, and Parameters
/// without requiring physical hardware.
/// </summary>
public class MockedDeviceIntegrationTests
{
    private Mock<IPowerSupplyHal> CreateMockHal()
    {
        var mockHal = new Mock<IPowerSupplyHal>();
        mockHal.Setup(h => h.MaxVoltageInVolts).Returns(30.0);
        mockHal.Setup(h => h.MaxCurrentInAmpere).Returns(5.0);
        mockHal.Setup(h => h.NumberOfChannels).Returns(1);
        mockHal.Setup(h => h.Identification).Returns("MockedPowerSupply");
        mockHal.Setup(h => h.Mode).Returns(new OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode[1]);
        
        mockHal.Setup(h => h.SetDesiredVolts(It.IsAny<double>(), It.IsAny<double>()))
            .Returns(OneDriver.Module.Definition.DeviceError.NoError);
        mockHal.Setup(h => h.SetDesiredAmps(It.IsAny<double>(), It.IsAny<double>()))
            .Returns(OneDriver.Module.Definition.DeviceError.NoError);
        mockHal.Setup(h => h.SetMode(It.IsAny<double>(), It.IsAny<OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode>()))
            .Returns(OneDriver.Module.Definition.DeviceError.NoError);
        mockHal.Setup(h => h.AllOn()).Returns(OneDriver.Module.Definition.DeviceError.NoError);
        mockHal.Setup(h => h.AllOff()).Returns(OneDriver.Module.Definition.DeviceError.NoError);
        
        return mockHal;
    }

    [Fact]
    public void Device_WithMockedHal_CreatesSuccessfully()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        
        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        Assert.NotNull(device);
        Assert.Equal("TestDevice", device.Parameters.Name);
        Assert.Single(device.Elements);
    }

    [Fact]
    public void Device_WithMockedHal_ReadsSpecificationsFromHal()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        
        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        Assert.Equal(30.0, device.Parameters.MaxVolts);
        Assert.Equal(5.0, device.Parameters.MaxAmps);
    }

    [Fact]
    public void Device_SetDesiredVolts_CallsHalSetDesiredVolts()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        var channel = device.Elements.First();
        channel.Parameters.DesiredVolts = 12.5;

        mockHal.Verify(h => h.SetDesiredVolts(0, 12.5), Times.Once);
    }

    [Fact]
    public void Device_SetDesiredAmps_CallsHalSetDesiredAmps()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        var channel = device.Elements.First();
        channel.Parameters.DesiredAmps = 2.5;

        mockHal.Verify(h => h.SetDesiredAmps(0, 2.5), Times.Once);
    }

    [Fact]
    public void Device_SetControlMode_CallsHalSetMode()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        var channel = device.Elements.First();
        channel.Parameters.ControlMode = OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode.Current;

        mockHal.Verify(h => h.SetMode(0, OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode.Current), Times.Once);
    }

    [Fact]
    public void Device_AllChannelsOn_CallsHalAllOn()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        var result = device.AllChannelsOn();

        Assert.Equal(0, result);
        mockHal.Verify(h => h.AllOn(), Times.Once);
    }

    [Fact]
    public void Device_AllChannelsOff_CallsHalAllOff()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        var result = device.AllChannelsOff();

        Assert.Equal(0, result);
        mockHal.Verify(h => h.AllOff(), Times.Once);
    }

    [Fact]
    public void Device_MultipleParameterChanges_CallsHalMultipleTimes()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        var channel = device.Elements.First();
        channel.Parameters.DesiredVolts = 10.0;
        channel.Parameters.DesiredAmps = 2.0;
        channel.Parameters.ControlMode = OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode.Current;

        mockHal.Verify(h => h.SetDesiredVolts(0, 10.0), Times.Once);
        mockHal.Verify(h => h.SetDesiredAmps(0, 2.0), Times.Once);
        mockHal.Verify(h => h.SetMode(0, OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode.Current), Times.Once);
    }

    [Fact]
    public void Device_SetVoltsAboveMaximum_IsRejectedWithoutCallingHal()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        var channel = device.Elements.First();
        var originalValue = channel.Parameters.DesiredVolts;

        // Validation exception is caught by SetProperty, value should remain unchanged
        channel.Parameters.DesiredVolts = 35.0;

        Assert.Equal(originalValue, channel.Parameters.DesiredVolts);
        Assert.NotEqual(35.0, channel.Parameters.DesiredVolts);
        mockHal.Verify(h => h.SetDesiredVolts(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Device_SetAmpsAboveMaximum_IsRejectedWithoutCallingHal()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        var channel = device.Elements.First();
        var originalValue = channel.Parameters.DesiredAmps;

        // Validation exception is caught by SetProperty, value should remain unchanged
        channel.Parameters.DesiredAmps = 10.0;

        Assert.Equal(originalValue, channel.Parameters.DesiredAmps);
        Assert.NotEqual(10.0, channel.Parameters.DesiredAmps);
        mockHal.Verify(h => h.SetDesiredAmps(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Device_ProcessData_IsNotNull()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        var channel = device.Elements.First();
        Assert.NotNull(channel.ProcessData);
    }

    [Fact]
    public void Device_WithCustomHalSpecifications_ReflectsCustomValues()
    {
        var mockHal = new Mock<IPowerSupplyHal>();
        mockHal.Setup(h => h.MaxVoltageInVolts).Returns(60.0);
        mockHal.Setup(h => h.MaxCurrentInAmpere).Returns(10.0);
        mockHal.Setup(h => h.NumberOfChannels).Returns(2);
        mockHal.Setup(h => h.Mode).Returns(new OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode[2]);
        
        var mockValidator = new Mock<IValidator>();
        var device = new Device("CustomDevice", mockValidator.Object, mockHal.Object);

        Assert.Equal(60.0, device.Parameters.MaxVolts);
        Assert.Equal(10.0, device.Parameters.MaxAmps);
        Assert.Equal(2, device.Elements.Count);
    }

    [Fact]
    public void Device_MultipleChannels_EachChannelIndependent()
    {
        var mockHal = new Mock<IPowerSupplyHal>();
        mockHal.Setup(h => h.MaxVoltageInVolts).Returns(30.0);
        mockHal.Setup(h => h.MaxCurrentInAmpere).Returns(5.0);
        mockHal.Setup(h => h.NumberOfChannels).Returns(2);
        mockHal.Setup(h => h.Mode).Returns(new OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode[2]);
        mockHal.Setup(h => h.SetDesiredVolts(It.IsAny<double>(), It.IsAny<double>()))
            .Returns(OneDriver.Module.Definition.DeviceError.NoError);
        
        var mockValidator = new Mock<IValidator>();
        var device = new Device("MultiChannel", mockValidator.Object, mockHal.Object);

        device.Elements[0].Parameters.DesiredVolts = 10.0;
        device.Elements[1].Parameters.DesiredVolts = 20.0;

        Assert.Equal(10.0, device.Elements[0].Parameters.DesiredVolts);
        Assert.Equal(20.0, device.Elements[1].Parameters.DesiredVolts);
        
        mockHal.Verify(h => h.SetDesiredVolts(0, 10.0), Times.Once);
        mockHal.Verify(h => h.SetDesiredVolts(1, 20.0), Times.Once);
    }

    [Fact]
    public void Device_SequentialOperations_IntegrationFlow()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        var channel = device.Elements.First();
        channel.Parameters.DesiredVolts = 15.0;
        channel.Parameters.DesiredAmps = 3.0;
        channel.Parameters.ControlMode = OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode.Current;

        device.AllChannelsOn();
        device.SetVolts(0, 20.0);
        device.SetAmps(0, 4.0);
        device.AllChannelsOff();

        mockHal.Verify(h => h.SetDesiredVolts(0, 15.0), Times.Once);
        mockHal.Verify(h => h.SetDesiredAmps(0, 3.0), Times.Once);
        mockHal.Verify(h => h.SetMode(0, Abstract.Contracts.Definition.ControlMode.Current), Times.Once);
        mockHal.Verify(h => h.AllOn(), Times.Once);
        mockHal.Verify(h => h.AllOff(), Times.Once);
    }

    [Fact]
    public void Device_ValidatorIntegration_UsedInConnectMethod()
    {
        var mockHal = CreateMockHal();
        var mockValidator = new Mock<IValidator>();
        mockValidator.Setup(v => v.Validate(It.IsAny<string>())).Returns(true);

        var device = new Device("TestDevice", mockValidator.Object, mockHal.Object);

        var result = device.Connect("COM3;9600");

        mockValidator.Verify(v => v.Validate("COM3;9600"), Times.Once);
    }
}
