using OneDriver.Framework.Libs.Announcer;

namespace OneDriver.PowerSupply.Basic.Products
{
    public class InternalDataHal : BaseDataForAnnouncement
    {
        public InternalDataHal(int channelNumber, double voltage, double current)
        {
            ChannelNumber = channelNumber;
            CurrentVoltage = voltage;
            CurrentCurrent = current;
            TimeStamp = DateTime.Now;
        }

        public InternalDataHal()
        {
            CurrentVoltage = 0;
            CurrentCurrent = 0;
            ChannelNumber = 0;
            TimeStamp = DateTime.Now;
        }

        public int ChannelNumber { get; }
        public double CurrentVoltage { get; }
        public double CurrentCurrent { get; }
    }
}
