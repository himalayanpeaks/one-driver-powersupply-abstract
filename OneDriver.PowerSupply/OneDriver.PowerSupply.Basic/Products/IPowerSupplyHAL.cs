using OneDriver.Module.Hal;

namespace OneDriver.PowerSupply.Basic.Products
{
    public interface IPowerSupplyHal : IHalLayer<InternalDataHal>, IStringReader, IStringWriter
    {
        public string Identification { get; }
        public Abstract.Contracts.Definition.ControlMode[] Mode { get; }
        public Module.Definition.DeviceError SetMode(double channelNumber, Abstract.Contracts.Definition.ControlMode mode);
        public double MaxCurrentInAmpere { get; }
        public double MaxVoltageInVolts { get; }
        public string GetErrorMessage(int code);
        public Module.Definition.DeviceError SetDesiredVolts(double channelNumber, double volts);
        public Module.Definition.DeviceError GetActualVolts(double channelNumber, out double volts);
        public Module.Definition.DeviceError SetDesiredAmps(double channelNumber, double amps);
        public Module.Definition.DeviceError GetActualAmps(double channelNumber, out double amps);
        public Module.Definition.DeviceError AllOff();
        public Module.Definition.DeviceError AllOn();
    }
}
