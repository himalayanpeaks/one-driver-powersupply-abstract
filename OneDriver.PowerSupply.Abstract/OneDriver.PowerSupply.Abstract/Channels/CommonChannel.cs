using OneDriver.Module.Channel;

namespace OneDriver.PowerSupply.Abstract.Channels
{
    /// <summary>
    /// Unused class
    /// </summary>
    /// <typeparam name="TChannelParams"></typeparam>
    /// <typeparam name="TChannelProcessData"></typeparam>
    public class CommonChannel<TChannelParams, TChannelProcessData>(
        TChannelParams parameters,
        TChannelProcessData processData) : BaseChannel<TChannelParams, TChannelProcessData>(parameters, processData)
        where TChannelParams : CommonChannelParams
        where TChannelProcessData : CommonProcessData;
}
