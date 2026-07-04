using OneDevice.Framework.Libs.Announcer;
using System.IO.Ports;
using OneDevice.Framework.Libs.Validator;
using Serilog;
using System.Globalization;
using OneDriver.Toolbox;

namespace OneDevice.PowerSupply.Basic.Products
{
    public class Kd3005p : DataTunnel<InternalDataHal>, IPowerSupplyHal
    {
        public Kd3005p()
        {
            ComPort = new SerialPort
            {
                ReadTimeout = 1000,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                BaudRate = 9600
            };
            MaxCurrentInAmpere = 5;
            MaxVoltageInVolts = 30;
            NumberOfChannels = 1;
            Mode = new Abstract.Contracts.Definition.ControlMode[NumberOfChannels];
            Identification = "KORAD KD3005P";
        }

        protected override void FetchDataForTunnel(ref InternalDataHal data)
        {
            for (var i = 0; i < NumberOfChannels; i++)
            {
                GetActualVolts(i, out var volts);
                Tools.Wait(500);
                GetActualAmps(i, out var amps);
                if(Math.Abs(volts - _previousVolts) > 0.05 || Math.Abs(amps - _previousAmps) > 0.05)
                    data = new InternalDataHal(i, volts, amps);
                _previousVolts = volts;
                _previousAmps = amps;    
            }
        }

        private double _previousVolts;
        private double _previousAmps;
        public Module.Definition.DeviceError Read(out string readData)
        {
            readData = "";
            try
            {
                ComPort.DiscardOutBuffer();
                readData = ComPort.ReadLine().ToString(new CultureInfo("en-EN"));
                Thread.Sleep(300);
            }
            catch (TimeoutException e)
            {
                Log.Error(e.ToString());
                return Module.Definition.DeviceError.Timeout;
            }
            catch (InvalidOperationException e)
            {
                Log.Error(e.ToString());
                return Module.Definition.DeviceError.ConnectionError;
            }
            catch (ArgumentException e)
            {
                Log.Error(e.ToString());
                return Module.Definition.DeviceError.DataIsNull;
            }
            return Module.Definition.DeviceError.NoError;
        }

        public Module.Definition.DeviceError Write(string data)
        {
            try
            {
                //ComPort.DiscardInBuffer();
                ComPort.WriteLine(data);
                Thread.Sleep(300);

            }
            catch (TimeoutException e)
            {
                Log.Error(e.ToString());
                return Module.Definition.DeviceError.Timeout;
            }
            catch (InvalidOperationException e)
            {
                Log.Error(e.ToString());
                return Module.Definition.DeviceError.ConnectionError;
            }
            catch(ArgumentException e)
            {
                Log.Error(e.ToString());
                return Module.Definition.DeviceError.DataIsNull;
            }
            return Module.Definition.DeviceError.NoError;
        }

        private SerialPort ComPort { get; set; }

        public Module.Definition.ConnectionError Open(string initString, IValidator validator)
        {
            ComPort.PortName = validator.ValidationRegex.Match(initString).Groups[1].Value;
            try
            {
                if (!ComPort.IsOpen)
                {
                    ComPort.Open();
                    Write("*IDN?");
                    if((Read(out var identification) is var err) && err != Module.Definition.DeviceError.NoError)
                    {
                        Log.Error(ComPort.PortName + " " + Module.Definition.ConnectionError.CommunicationError.GetDescription());
                        return Module.Definition.ConnectionError.CommunicationError;
                    }
                    StartProcessDataAnnouncer();
                }
                else
                    Log.Error(ComPort.PortName + " " + Module.Definition.ConnectionError.AlreadyOpened.GetDescription());

            }
            catch (ArgumentException)
            {
                Log.Error(ComPort.PortName + " " + Module.Definition.ConnectionError.InvalidName.GetDescription());
                return Module.Definition.ConnectionError.InvalidName;
            }
            catch (UnauthorizedAccessException)
            {
                Log.Error(ComPort.PortName + " " + Module.Definition.ConnectionError.UnauthorizedAccess.GetDescription());
                return Module.Definition.ConnectionError.UnauthorizedAccess;
            }
            catch (IOException)
            {
                Log.Error(ComPort.PortName + " " + Module.Definition.ConnectionError.IOError.GetDescription());
                return Module.Definition.ConnectionError.IOError;
            }
            catch (InvalidOperationException)
            {
                Log.Error(ComPort.PortName + " " + Module.Definition.ConnectionError.InvalidOperation.GetDescription());
                return Module.Definition.ConnectionError.InvalidOperation;
            }
            catch (Exception)
            {
                Log.Error(ComPort.PortName + " " + Module.Definition.ConnectionError.UnknownError.GetDescription());
                return Module.Definition.ConnectionError.UnknownError;
            }

            return Module.Definition.ConnectionError.NoError;
        }

        public Module.Definition.ConnectionError Close()
        {
            try
            {
                ComPort.Close();
            }
            catch (Exception)
            {
                Log.Error(ComPort.PortName + " " + Module.Definition.ConnectionError.ErrorInDisconnecting.GetDescription());
                return Module.Definition.ConnectionError.ErrorInDisconnecting;
            }
            return Module.Definition.ConnectionError.NoError;
        }

        public double MaxCurrentInAmpere { get; }
        public double MaxVoltageInVolts { get; }
        public string Identification { get; private set; }
        public Abstract.Contracts.Definition.ControlMode[] Mode { get; }
        public Module.Definition.DeviceError SetMode(double channelNumber, Abstract.Contracts.Definition.ControlMode mode)
        {
            return Module.Definition.DeviceError.NoError;
        }
        public string GetErrorMessage(int code)
        {
            if (Enum.IsDefined(typeof(Module.Definition.DeviceError), code))
                return ((Module.Definition.DeviceError)code).ToString();

            return $"Unknown error code: {code}";
        }


        public Module.Definition.DeviceError SetDesiredVolts(double channelNumber, double volts)
        {
            var str = "VSET" + (channelNumber + 1) + ":" + volts.ToString(new CultureInfo("en-EN"));
            var err = Write(str);
            if (err != Module.Definition.DeviceError.NoError)
                return err;
            else
                Write("OUT" + (channelNumber + 1));

            return Module.Definition.DeviceError.NoError;
        }

        public Module.Definition.DeviceError GetActualVolts(double channelNumber, out double volts)
        {
            volts = 0;
            string command = $"VOUT{channelNumber + 1}?";

            if (Write(command) is var err && err != Module.Definition.DeviceError.NoError)
                return err;
            
            if (Read(out var val) is var readErr && readErr != Module.Definition.DeviceError.NoError)
                return readErr;

            if (double.TryParse(val, NumberStyles.Float, new CultureInfo("en-EN"), out volts))
                return Module.Definition.DeviceError.NoError;
            Log.Error(val + " : Invalid response from device.");
            return Module.Definition.DeviceError.InvalidResponse; 
        }
        public Module.Definition.DeviceError GetActualAmps(double channelNumber, out double amps)
        {
            amps = 0;
            string command = $"IOUT{channelNumber + 1}?";

            if (Write(command) is var err && err != Module.Definition.DeviceError.NoError)
                return err;
            if (Read(out var val) is var readErr && readErr != Module.Definition.DeviceError.NoError)
                return readErr;

            if (double.TryParse(val, NumberStyles.Float, new CultureInfo("en-EN"), out amps))
                return Module.Definition.DeviceError.NoError;
            Log.Error(val + " : Invalid response from device.");
            return Module.Definition.DeviceError.InvalidResponse;
        }

        public Module.Definition.DeviceError SetDesiredAmps(double channelNumber, double amps)
        {
            var str = "ISET" + (channelNumber + 1) + ":" + amps.ToString(new CultureInfo("en-EN"));
            var err = Write(str);
            if(err != Module.Definition.DeviceError.NoError)
                return err;
            else
                Write("OUT" + (channelNumber + 1));
            return Module.Definition.DeviceError.NoError;
        }

        public Module.Definition.DeviceError AllOff()
        {
            return Write("OUT0");
        }

        public Module.Definition.DeviceError AllOn()
        {
            return Write("OUT1");
        }

        public void StartProcessDataAnnouncer() => StartAnnouncingData();

        public void StopProcessDataAnnouncer() => StopAnnouncingData();

        public void AttachToProcessDataEvent(DataEventHandler processDataEventHandler)
            => DataEvent += processDataEventHandler;

        public int NumberOfChannels { get; }
    }
}
