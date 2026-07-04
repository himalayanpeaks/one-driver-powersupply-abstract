using OneDevice.Module.Parameter;
using OneDevice.PowerSupply.Abstract.Contracts;

namespace OneDevice.PowerSupply.Abstract.Channels
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
            set => SetProperty<Definition.ControlMode>(ref _controlMode, value);
        }
    }
}
