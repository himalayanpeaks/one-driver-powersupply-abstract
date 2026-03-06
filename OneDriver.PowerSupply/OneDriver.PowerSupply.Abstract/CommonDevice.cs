using OneDriver.Framework.Libs.Validator;
using OneDriver.Module.Channel;
using OneDriver.Module.Device;
using OneDriver.PowerSupply.Abstract.Channels;
using OneDriver.PowerSupply.Abstract.Contracts;
using System.Collections.ObjectModel;

namespace OneDriver.PowerSupply.Abstract
{
    public abstract class CommonDevice<TDeviceParams, TChannelParams, TChannelProcessData>(
        TDeviceParams parameters,
        IValidator validator,
        ObservableCollection<BaseChannel<TChannelParams, TChannelProcessData>> elements)
        :
            BaseDeviceWithChannelsHavingProcessData<TDeviceParams, TChannelParams, TChannelProcessData>(parameters,
                validator, elements), IPowerSupplyFunctions
        where TDeviceParams : CommonDeviceParams
        where TChannelParams : CommonChannelParams
        where TChannelProcessData : CommonProcessData
    {
        public abstract int AllChannelsOff();
        public abstract int SetVolts(int channelNumber, double volts);
        public abstract int SetAmps(int channelNumber, double amps);
        public abstract int AllChannelsOn();
    }
}
