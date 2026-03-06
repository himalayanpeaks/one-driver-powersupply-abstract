using OneDriver.Module.Parameter;

namespace OneDriver.PowerSupply.Abstract
{
    public class CommonDeviceParams : BaseDeviceWithChannelsParams
    {
        public double MaxAmps => GetProperty<double>();
        public double MaxVolts => GetProperty<double>();
        public CommonDeviceParams(string name) : base(name)
        {
            this.PropertyReadRequested += CommonDeviceParams_PropertyReadRequested;
        }

        private void CommonDeviceParams_PropertyReadRequested(object sender, Framework.Base.PropertyReadRequestedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MaxVolts):
                    e.Value = 0.0;
                    break;
                case nameof(MaxAmps):
                    e.Value = 0.0;
                    break;
            }
        }
    }
}
