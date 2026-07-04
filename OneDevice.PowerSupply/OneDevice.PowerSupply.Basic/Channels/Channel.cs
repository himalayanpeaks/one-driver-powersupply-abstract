using OneDevice.Module.Channel;
using OneDevice.PowerSupply.Abstract.Channels;

namespace OneDevice.PowerSupply.Basic.Channels
{
    /// <summary>
    /// Unused class
    /// </summary>
    public class Channel(ChannelParams parameters, ChannelProcessData processData)
        : BaseChannel<ChannelParams, ChannelProcessData>(parameters, processData);
}
