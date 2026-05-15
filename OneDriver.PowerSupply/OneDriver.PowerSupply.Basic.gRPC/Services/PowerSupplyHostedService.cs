using OneDriver.PowerSupply.Factory;
using OneDriver.PowerSupply.Abstract;
using OneDriver.PowerSupply.Abstract.Channels;

namespace OneDriver.PowerSupply.Basic.gRPC.Services
{
    public class PowerSupplyHostedService : IHostedService
    {
        private readonly ILogger<PowerSupplyHostedService> _logger;
        private readonly IConfiguration _configuration;
        private readonly PowerSupplyServiceImpl _powerSupplyService;
        private readonly AzureIoTHubService _iotHubService;
        private readonly CloudCommandHandler _commandHandler;

        public PowerSupplyHostedService(
            ILogger<PowerSupplyHostedService> logger,
            IConfiguration configuration,
            PowerSupplyServiceImpl powerSupplyService,
            AzureIoTHubService iotHubService,
            CloudCommandHandler commandHandler)
        {
            _logger = logger;
            _configuration = configuration;
            _powerSupplyService = powerSupplyService;
            _iotHubService = iotHubService;
            _commandHandler = commandHandler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var autoConnect = _configuration.GetValue<bool>("PowerSupply:AutoConnectOnStartup");
                if (!autoConnect)
                {
                    _logger.LogInformation("Auto-connect is disabled");
                    return;
                }

                _logger.LogInformation("Creating Power Supply device...");
                var productType = _configuration["PowerSupply:ProductType"] ?? "KD3005P";
                var deviceName = _configuration["PowerSupply:DeviceName"] ?? "PowerSupply1";
                var deviceId = _configuration["PowerSupply:DefaultDeviceId"] ?? "ps-001";

                // Create device based on product type
                PowerSupplyType psType = productType.ToUpperInvariant() switch
                {
                    "KD3005P" => PowerSupplyType.Kd3005p,
                    _ => PowerSupplyType.Kd3005p
                };

                var device = PowerSupplyFactory.Create(psType);

                if (device == null)
                {
                    _logger.LogError("Failed to create Power Supply device");
                    return;
                }

                _powerSupplyService.RegisterDevice(deviceId, device);
                _logger.LogInformation("Device registered with ID: {DeviceId}", deviceId);

                _logger.LogInformation("Connecting to Power Supply device...");
                var comPort = _configuration["PowerSupply:ComPort"] ?? "COM5";
                var errorCode = device.Connect(comPort);

                if (errorCode != 0)
                {
                    _logger.LogError("Failed to connect to Power Supply: Error code {ErrorCode}", errorCode);
                    return;
                }

                _logger.LogInformation("Successfully connected to Power Supply at {ComPort}", comPort);
                _logger.LogInformation("Device has {ChannelCount} channel(s)", device.Elements.Count);
                _logger.LogInformation("Max Voltage: {MaxVolts}V, Max Current: {MaxAmps}A", 
                    device.Parameters.MaxVolts, device.Parameters.MaxAmps);

                // Set up Azure IoT Hub integration
                _logger.LogInformation("Setting power supply service reference for Direct Methods...");
                _iotHubService.SetPowerSupplyService(_powerSupplyService);

                _logger.LogInformation("Starting Cloud-to-Device command listener...");
                await _iotHubService.StartReceivingCommandsAsync();
                _logger.LogInformation("Cloud command handler is ready. PowerSupply {DeviceId} ready to receive commands from Azure.", deviceId);

                // Send initial telemetry
                for (int i = 0; i < device.Elements.Count; i++)
                {
                    var processData = (CommonProcessData)device.Elements[i].ProcessData;
                    await _iotHubService.SendTelemetryAsync(deviceId, i, processData.Voltage, processData.Current);
                }
                _logger.LogInformation("Initial telemetry sent to Azure IoT Hub");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Power Supply initialization");
            }

            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Power Supply service");
            return Task.CompletedTask;
        }
    }
}
