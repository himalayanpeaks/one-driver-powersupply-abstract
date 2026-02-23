using OneDriver.Module.Parameter;
using OneDriver.PowerSupply.Abstract.Contracts;

namespace OneDriver.PowerSupply.Abstract.Channels
{
    public class CommonChannelParams(string name) : BaseChannelParams(name)
    {
        private double _desiredVolts;
        private double _desiredAmps;
        private Definition.ControlMode _controlMode;

        public double DesiredVolts
        {
            get => _desiredVolts;
            set => SetProperty(ref _desiredVolts, value);
        }

        public double DesiredAmps
        {
            get => _desiredAmps;
            set => SetProperty(ref _desiredAmps, value);
        }

        public Definition.ControlMode ControlMode
        {
            get => _controlMode;
            set => SetProperty(ref _controlMode, value);
        }
    }
}
