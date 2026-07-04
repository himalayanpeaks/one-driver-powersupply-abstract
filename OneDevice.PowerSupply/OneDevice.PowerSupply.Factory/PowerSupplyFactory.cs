using OneDevice.PowerSupply.Abstract.Channels;
using OneDevice.PowerSupply.Abstract;
using OneDevice.Framework.Libs.Validator;
using OneDevice.PowerSupply.Basic.Products;

namespace OneDevice.PowerSupply.Factory
{
    public enum PowerSupplyType
    {
        Virtual,
        Kd3005p
    }
    public class PowerSupplyFactory
    {
        public static CommonDevice<CommonDeviceParams, CommonChannelParams, CommonProcessData>? Create(PowerSupplyType deviceType)
        {
            switch (deviceType)
            {
                case PowerSupplyType.Virtual:
                    break;
                case PowerSupplyType.Kd3005p:
                    return new Basic.Device("PowerSupplyVirtual", new ComportValidator(), new Kd3005p());
                    
            }
            return null;
        }
    }
}
