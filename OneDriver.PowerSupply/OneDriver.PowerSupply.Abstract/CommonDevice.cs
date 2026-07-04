using OneDevice.Framework.Libs.Validator;
using OneDevice.Module.Channel;
using OneDevice.Module.Device;
using OneDevice.PowerSupply.Abstract.Channels;
using OneDevice.PowerSupply.Abstract.Contracts;
using System.Collections.ObjectModel;

namespace OneDevice.PowerSupply.Abstract
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
