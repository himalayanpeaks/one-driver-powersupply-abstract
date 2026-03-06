using Moq;
using OneDriver.PowerSupply.Basic;
using OneDriver.PowerSupply.Basic.Products;
using OneDriver.Framework.Libs.Validator;

namespace OneDriver.PowerSupply.IntegrationTest;

/// <summary>
/// Helper class for creating mocked devices for integration tests.
/// Provides common setup methods to avoid code duplication.
/// </summary>
public static class MockDeviceHelper
{
    public static Device CreateMockedDevice(string name = "TestDevice", double maxVolts = 30.0, double maxAmps = 5.0, int channelCount = 1)
    {
        var mockHal = new Mock<IPowerSupplyHal>();
        mockHal.Setup(h => h.MaxVoltageInVolts).Returns(maxVolts);
        mockHal.Setup(h => h.MaxCurrentInAmpere).Returns(maxAmps);
        mockHal.Setup(h => h.NumberOfChannels).Returns(channelCount);
        mockHal.Setup(h => h.Identification).Returns("MockedPowerSupply");
        mockHal.Setup(h => h.Mode).Returns(new OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode[channelCount]);
        mockHal.Setup(h => h.SetDesiredVolts(It.IsAny<double>(), It.IsAny<double>()))
            .Returns(OneDriver.Module.Definition.DeviceError.NoError);
        mockHal.Setup(h => h.SetDesiredAmps(It.IsAny<double>(), It.IsAny<double>()))
            .Returns(OneDriver.Module.Definition.DeviceError.NoError);
        mockHal.Setup(h => h.SetMode(It.IsAny<double>(), It.IsAny<OneDriver.PowerSupply.Abstract.Contracts.Definition.ControlMode>()))
            .Returns(OneDriver.Module.Definition.DeviceError.NoError);
        mockHal.Setup(h => h.AllOn()).Returns(OneDriver.Module.Definition.DeviceError.NoError);
        mockHal.Setup(h => h.AllOff()).Returns(OneDriver.Module.Definition.DeviceError.NoError);

        var mockValidator = new Mock<IValidator>();
        return new Device(name, mockValidator.Object, mockHal.Object);
    }
}
