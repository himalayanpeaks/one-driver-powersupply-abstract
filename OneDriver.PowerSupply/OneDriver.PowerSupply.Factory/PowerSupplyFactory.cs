using OneDriver.PowerSupply.Abstract.Channels;
using OneDriver.PowerSupply.Abstract;
using OneDriver.Framework.Libs.Validator;
using OneDriver.PowerSupply.Basic.Products;

namespace OneDriver.PowerSupply.Factory
{
    public enum PowerSupplyType
    {
        Virtual,
        Kd3005p
    }
    /*public class AbstractPowerSupply
    {
        public CommonDeviceParams Parameters { get;  }

        public AbstractPowerSupply(CommonDeviceParams parameters, IPowerSupplyFunctions methods, ObservableCollection<BaseChannelWithProcessData<CommonChannelParams, CommonProcessData>> elements)
        {
            Parameters = parameters;
            Methods = methods;
            Elements = elements;
        }

        public IPowerSupplyFunctions Methods { get;  }
        public ObservableCollection<CommonChannel<CommonChannelParams, CommonProcessData>> Elements { get; }
    }*/

    public class PowerSupplyFactory
    {
        public static CommonDevice<CommonDeviceParams, CommonChannelParams, CommonProcessData> Create(PowerSupplyType deviceType)
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
