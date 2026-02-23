using OneDriver.Module.Function;

namespace OneDriver.PowerSupply.Abstract.Contracts
{
    public interface IPowerSupplyFunctions : IFunctions
    {
        int SetVolts(int channelNumber, double volts);
        int SetAmps(int channelNumber, double amps);
        int AllChannelsOn();
        int AllChannelsOff();
    }
}
