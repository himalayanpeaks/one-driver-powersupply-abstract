namespace OneDriver.PowerSupply.Basic.Products
{
    public class Definition
    {

        public enum ErrorState
        {
            NoError,
            OverVoltage,
            OverCurrent,
            CommunicationError,
            Unknown
        }
    }
}
