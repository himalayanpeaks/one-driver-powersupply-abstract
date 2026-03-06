using System.ComponentModel;
using OneDriver.PowerSupply.Abstract.Channels;

namespace OneDriver.PowerSupply.Basic.Channels
{
    public class ChannelProcessData : CommonProcessData
    {
        private double _volts;
        private double _curr;

        internal double Volts
        {
            get => _volts;
            set => SetProperty(ref _volts, value);
        }

        internal double Curr
        {
            get => _curr;
            set => SetProperty(ref _curr, value);
        }

        internal DateTime CurrentTime { get; set; }

        public ChannelProcessData()
        {
            this.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Volts):
                case nameof(Curr):
                case nameof(CurrentTime):

                    if (sender != null)
                    {

                        this.Voltage = (((ChannelProcessData)sender)).Volts;
                        this.Current = (((ChannelProcessData)sender)).Curr;
                        this.TimeStamp = DateTime.Now;
                    }

                    break;
            }
        }
    }
}
