using OneDriver.Module.Channel;
using OneDriver.PowerSupply.Abstract.Channels;

namespace OneDriver.PowerSupply.Basic.Channels
{
    /// <summary>
    /// Unused class
    /// </summary>
    public class Channel(ChannelParams parameters, ChannelProcessData processData)
        : BaseChannel<ChannelParams, ChannelProcessData>(parameters, processData);
}
