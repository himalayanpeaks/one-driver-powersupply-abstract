using OneDriver.Framework.Base;
using OneDriver.Framework.Libs.Validator;
using OneDriver.PowerSupply.Abstract;
using OneDriver.PowerSupply.Basic.Channels;
using OneDriver.PowerSupply.Basic.Products;
using System.Collections.ObjectModel;
using System.ComponentModel;
using OneDriver.Module.Channel;
using OneDriver.PowerSupply.Abstract.Channels;
using Serilog;

namespace OneDriver.PowerSupply.Basic
{
    public class Device : CommonDevice<CommonDeviceParams, CommonChannelParams, CommonProcessData>
    {
        public Device(string name, IValidator validator, IPowerSupplyHal powerSupplyHal) :
            base(new DeviceParams(name), validator, 
                new ObservableCollection<BaseChannel<CommonChannelParams, CommonProcessData>>())
        {
            _powerSupplyHal = powerSupplyHal;
            Init();
        }

        private void Init()
        {
            Parameters.PropertyChanging += Parameters_PropertyChanging;
            Parameters.PropertyChanged += Parameters_PropertyChanged;
            Parameters.PropertyReadRequested += Parameters_PropertyReadRequested;
            _powerSupplyHal.AttachToProcessDataEvent(ProcessDataChanged);


            for (var i = 0; i < _powerSupplyHal.NumberOfChannels; i++)
            {
                var item = new CommonChannel<CommonChannelParams, CommonProcessData>(new ChannelParams("Ch" + i.ToString()), new ChannelProcessData());
                
                item.Parameters.PropertyChanged += Parameters_PropertyChanged;
                item.Parameters.PropertyChanging += Parameters_PropertyChanging;
                item.Parameters.PropertyReadRequested += Parameters_PropertyReadRequested;
                Elements.Add(item);
            }   
        }

        private void Parameters_PropertyReadRequested(object sender, PropertyReadRequestedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Parameters.MaxVolts):
                    e.Value = _powerSupplyHal.MaxVoltageInVolts;
                    break;
                case nameof(Parameters.MaxAmps):
                    e.Value = _powerSupplyHal.MaxCurrentInAmpere;
                    break;
                case nameof(ChannelProcessData.TimeStamp):
                    e.Value = ((InternalDataHal)sender).TimeStamp;
                    break;
            }
        }

        private void ProcessDataChanged(object sender, InternalDataHal e)
        {
            ((ChannelProcessData)Elements[e.ChannelNumber].ProcessData).Curr= e.CurrentCurrent;
            ((ChannelProcessData)Elements[e.ChannelNumber].ProcessData).Volts = e.CurrentVoltage;
        }

        private void Parameters_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            int index = -1;
            switch (e.PropertyName)
            {
                case nameof(ChannelParams.DesiredAmps):
                    var channel = this.Elements.FirstOrDefault(x => sender != null && x.Parameters == (ChannelParams)sender);
                    if (channel != null) index = this.Elements.IndexOf(channel);
                    if (sender != null) _powerSupplyHal.SetDesiredAmps(index, (((ChannelParams)sender).DesiredAmps));
                    break;
                case nameof(ChannelParams.DesiredVolts):
                    channel = this.Elements.FirstOrDefault(x => sender != null && x.Parameters == (ChannelParams)sender);
                    if (channel != null) index = this.Elements.IndexOf(channel);
                    if (sender != null) _powerSupplyHal.SetDesiredVolts(index, (((ChannelParams)sender).DesiredVolts));
                    break;
                case nameof(ChannelParams.ControlMode):
                    channel = this.Elements.FirstOrDefault(x => sender != null && x.Parameters == (ChannelParams)sender);
                    if (channel != null) index = this.Elements.IndexOf(channel);
                    if (sender != null) _powerSupplyHal.SetMode(index, (((ChannelParams)sender).ControlMode));
                    break;
            }
        }

        private void Parameters_PropertyChanging(object sender, PropertyValidationEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BaseChannel<ChannelParams, ChannelProcessData>.Parameters.DesiredAmps):
                    if ((double)e.NewValue > Parameters.MaxAmps || (double)e.NewValue < 0)
                    {
                        Log.Error("Desired Amps is greater than Max Amps");
                        throw new ArgumentOutOfRangeException(e.PropertyName);
                    }

                    break;
                case nameof(BaseChannel<ChannelParams, ChannelProcessData>.Parameters.DesiredVolts):
                    if ((double)e.NewValue > Parameters.MaxVolts || (double)e.NewValue < 0)
                    {
                        Log.Error("Desired Volts is greater than Max Volts");
                        throw new ArgumentOutOfRangeException(e.PropertyName);
                    }
                    break;
            }
        }

        private readonly IPowerSupplyHal _powerSupplyHal;
        protected override int CloseConnection() => (int)_powerSupplyHal.Close();

        protected override string GetErrorMessageFromDerived(int code)
        {
            throw new NotImplementedException();
        }

        protected override int OpenConnection(string initString) => (int)_powerSupplyHal.Open(initString, Validator);

        public override int AllChannelsOff() => (int)_powerSupplyHal.AllOff();

        public override int SetVolts(int channelNumber, double volts) => (int)_powerSupplyHal.SetDesiredVolts(channelNumber, volts);

        public override int SetAmps(int channelNumber, double amps) => (int)_powerSupplyHal.SetDesiredAmps(channelNumber, amps);

        public override int AllChannelsOn() => (int)_powerSupplyHal.AllOn();
    }
}
