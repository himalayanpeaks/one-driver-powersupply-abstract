using OneDriver.Module.Parameter;


namespace OneDriver.PowerSupply.Abstract.Channels
{
    public class CommonProcessData : BaseProcessData
    {
        private double _current;
        private double _voltage;

        public double Voltage
        {
            get => _voltage;
            protected set => SetProperty(ref _voltage, value);
        }

        public double Current
        {
            get => _current;
            protected set => SetProperty(ref _current, value);
        }
    }
}
