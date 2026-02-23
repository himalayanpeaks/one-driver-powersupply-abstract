using OneDriver.Module.Channel;

namespace OneDriver.PowerSupply.Abstract.Channels
{
    /// <summary>
    /// Unused class
    /// </summary>
    /// <typeparam name="TChannelParams"></typeparam>
    /// <typeparam name="TChannelProcessData"></typeparam>
    public class CommonChannel<TChannelParams, TChannelProcessData>
        : BaseChannel<TChannelParams, TChannelProcessData>
        where TChannelParams : CommonChannelParams
        where TChannelProcessData : CommonProcessData
    {
        protected CommonChannel(TChannelParams parameters, TChannelProcessData processData) : base(parameters, processData)
        {
        }
    }
}
