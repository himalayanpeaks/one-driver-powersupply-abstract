namespace OneDriver.PowerSupply.Basic.gRPC.Services
{
    public class CloudCommandHandler
    {
        private readonly ILogger<CloudCommandHandler> _logger;
        private readonly PowerSupplyServiceImpl _powerSupplyService;
        private readonly AzureIoTHubService _iotHubService;
        private readonly IConfiguration _configuration;

        public CloudCommandHandler(
            ILogger<CloudCommandHandler> logger,
            PowerSupplyServiceImpl powerSupplyService,
            AzureIoTHubService iotHubService,
            IConfiguration configuration)
        {
            _logger = logger;
            _powerSupplyService = powerSupplyService;
            _iotHubService = iotHubService;
            _configuration = configuration;

            _iotHubService.OnCommandReceived += HandleCommandAsync;
            _logger.LogInformation("CloudCommandHandler initialized and subscribed to command events");
        }

        private async Task HandleCommandAsync(CloudCommand command)
        {
            _logger.LogInformation("====> CloudCommandHandler: Processing command from cloud: Action={Action}, Channel={Channel}", 
                command.Action, command.ChannelNumber);

            try
            {
                var deviceId = string.IsNullOrEmpty(command.DeviceId) 
                    ? _configuration["PowerSupply:DefaultDeviceId"] ?? "ps-001" 
                    : command.DeviceId;

                _logger.LogInformation("Using DeviceId: {DeviceId}", deviceId);

                switch (command.Action.ToLowerInvariant())
                {
                    case "setvolts":
                    case "setvoltage":
                        _logger.LogInformation("Executing SET VOLTS command for channel {Channel}: {Volts}V", 
                            command.ChannelNumber, command.Volts);
                        await HandleSetVoltsAsync(deviceId, command.ChannelNumber, command.Volts);
                        break;

                    case "setamps":
                    case "setcurrent":
                        _logger.LogInformation("Executing SET AMPS command for channel {Channel}: {Amps}A", 
                            command.ChannelNumber, command.Amps);
                        await HandleSetAmpsAsync(deviceId, command.ChannelNumber, command.Amps);
                        break;

                    case "allchannelson":
                    case "allon":
                        _logger.LogInformation("Executing ALL CHANNELS ON command");
                        await HandleAllChannelsOnAsync(deviceId);
                        break;

                    case "allchannelsoff":
                    case "alloff":
                        _logger.LogInformation("Executing ALL CHANNELS OFF command");
                        await HandleAllChannelsOffAsync(deviceId);
                        break;

                    case "getstatus":
                    case "status":
                        _logger.LogInformation("Executing GET STATUS command");
                        await HandleGetStatusAsync(deviceId);
                        break;

                    default:
                        _logger.LogWarning("Unknown command action: {Action}", command.Action);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling cloud command");
            }
        }

        private async Task HandleSetVoltsAsync(string deviceId, int channelNumber, double volts)
        {
            var request = new SetVoltsRequest
            {
                DeviceId = deviceId,
                ChannelNumber = channelNumber,
                Volts = volts
            };

            var response = await _powerSupplyService.SetVolts(request, null!);

            await _iotHubService.SendCommandResultAsync(
                deviceId,
                "setVolts",
                channelNumber,
                $"{volts}V",
                response.Result,
                response.Message
            );

            _logger.LogInformation("Set volts Ch{Channel}: {Volts}V (Result: {Result})", 
                channelNumber, volts, response.Result);
        }

        private async Task HandleSetAmpsAsync(string deviceId, int channelNumber, double amps)
        {
            var request = new SetAmpsRequest
            {
                DeviceId = deviceId,
                ChannelNumber = channelNumber,
                Amps = amps
            };

            var response = await _powerSupplyService.SetAmps(request, null!);

            await _iotHubService.SendCommandResultAsync(
                deviceId,
                "setAmps",
                channelNumber,
                $"{amps}A",
                response.Result,
                response.Message
            );

            _logger.LogInformation("Set amps Ch{Channel}: {Amps}A (Result: {Result})", 
                channelNumber, amps, response.Result);
        }

        private async Task HandleAllChannelsOnAsync(string deviceId)
        {
            var request = new AllChannelsOnRequest
            {
                DeviceId = deviceId
            };

            var response = await _powerSupplyService.AllChannelsOn(request, null!);

            await _iotHubService.SendCommandResultAsync(
                deviceId,
                "allChannelsOn",
                -1,
                "All channels ON",
                response.Result,
                response.Message
            );

            _logger.LogInformation("All channels ON (Result: {Result})", response.Result);
        }

        private async Task HandleAllChannelsOffAsync(string deviceId)
        {
            var request = new AllChannelsOffRequest
            {
                DeviceId = deviceId
            };

            var response = await _powerSupplyService.AllChannelsOff(request, null!);

            await _iotHubService.SendCommandResultAsync(
                deviceId,
                "allChannelsOff",
                -1,
                "All channels OFF",
                response.Result,
                response.Message
            );

            _logger.LogInformation("All channels OFF (Result: {Result})", response.Result);
        }

        private async Task HandleGetStatusAsync(string deviceId)
        {
            var request = new GetDeviceStatusRequest
            {
                DeviceId = deviceId
            };

            var response = await _powerSupplyService.GetDeviceStatus(request, null!);

            await _iotHubService.SendCommandResultAsync(
                deviceId,
                "getStatus",
                -1,
                $"Connected: {response.IsConnected}, Channels: {response.NumberOfChannels}",
                0,
                "Status retrieved successfully"
            );

            _logger.LogInformation("Device status retrieved: Connected={Connected}, Channels={Channels}", 
                response.IsConnected, response.NumberOfChannels);
        }
    }
}
