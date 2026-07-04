using Microsoft.Azure.Devices.Client;
using System.Text;
using System.Text.Json;

namespace OneDevice.PowerSupply.Basic.gRPC.Services
{
    public class AzureIoTHubService : IDisposable
    {
        private readonly ILogger<AzureIoTHubService> _logger;
        private readonly string? _connectionString;
        private DeviceClient? _deviceClient;
        private PowerSupplyServiceImpl? _powerSupplyService;

        public event Func<CloudCommand, Task>? OnCommandReceived;

        public AzureIoTHubService(IConfiguration configuration, ILogger<AzureIoTHubService> logger)
        {
            _logger = logger;
            _connectionString = configuration["AzureIoTHub:ConnectionString"]
                ?? Environment.GetEnvironmentVariable("AZURE_IOTHUB_DEVICE_CONNECTION_STRING");

            if (!string.IsNullOrEmpty(_connectionString))
            {
                try
                {
                    _deviceClient = DeviceClient.CreateFromConnectionString(_connectionString, TransportType.Mqtt);
                    _logger.LogInformation("Azure IoT Hub client initialized successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Azure IoT Hub client");
                }
            }
            else
            {
                _logger.LogWarning("Azure IoT Hub connection string not configured. Set AzureIoTHub:ConnectionString or AZURE_IOTHUB_DEVICE_CONNECTION_STRING.");
            }
        }

        public void SetPowerSupplyService(PowerSupplyServiceImpl powerSupplyService)
        {
            _powerSupplyService = powerSupplyService;
            _logger.LogInformation("PowerSupply service reference set for Direct Methods");
        }

        public async Task StartReceivingCommandsAsync()
        {
            if (_deviceClient == null || string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogWarning("IoT Hub client not initialized - cannot receive commands");
                return;
            }

            try
            {
                // Set up C2D message handler
                await _deviceClient.SetReceiveMessageHandlerAsync(ReceiveC2dMessageAsync, null);
                _logger.LogInformation("Started listening for Cloud-to-Device messages from Azure");

                // Set up Direct Method handlers
                await _deviceClient.SetMethodHandlerAsync("SetVolts", HandleSetVoltsMethod, null);
                await _deviceClient.SetMethodHandlerAsync("SetAmps", HandleSetAmpsMethod, null);
                await _deviceClient.SetMethodHandlerAsync("AllChannelsOn", HandleAllChannelsOnMethod, null);
                await _deviceClient.SetMethodHandlerAsync("AllChannelsOff", HandleAllChannelsOffMethod, null);
                await _deviceClient.SetMethodHandlerAsync("GetDeviceStatus", HandleGetDeviceStatusMethod, null);
                await _deviceClient.SetMethodHandlerAsync("GetChannelParameters", HandleGetChannelParametersMethod, null);
                await _deviceClient.SetMethodDefaultHandlerAsync(HandleDefaultMethod, null);

                _logger.LogInformation("Started listening for Direct Methods from Azure");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start receiving commands");
            }
        }

        private async Task ReceiveC2dMessageAsync(Message receivedMessage, object? userContext)
        {
            try
            {
                var messageData = Encoding.UTF8.GetString(receivedMessage.GetBytes());
                _logger.LogInformation("Received C2D message from Azure: {Message}", messageData);

                var command = JsonSerializer.Deserialize<CloudCommand>(messageData, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                _logger.LogInformation("Deserialized command - Action: {Action}, Channel: {Channel}", 
                    command?.Action, command?.ChannelNumber);

                if (command != null && OnCommandReceived != null)
                {
                    _logger.LogInformation("Invoking command handler...");
                    await OnCommandReceived(command);
                    _logger.LogInformation("Command handler invoked successfully");
                }

                await _deviceClient!.CompleteAsync(receivedMessage);
                _logger.LogInformation("C2D message completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing C2D message");
                try
                {
                    await _deviceClient!.RejectAsync(receivedMessage);
                }
                catch { }
            }
        }

        public async Task<bool> SendTelemetryAsync(string deviceId, int channelNumber, double volts, double amps)
        {
            if (_deviceClient == null || string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogDebug("IoT Hub client not initialized - telemetry not sent");
                return false;
            }

            try
            {
                var telemetryData = new
                {
                    deviceId,
                    channelNumber,
                    volts,
                    amps,
                    timestamp = DateTimeOffset.UtcNow
                };

                var messageString = JsonSerializer.Serialize(telemetryData);

                var message = new Message(Encoding.UTF8.GetBytes(messageString))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8"
                };
                await _deviceClient.SendEventAsync(message);

                _logger.LogDebug("Telemetry sent to IoT Hub: {DeviceId}/Ch{Channel}", deviceId, channelNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send telemetry to IoT Hub");
                return false;
            }
        }

        public async Task<bool> SendCommandResultAsync(string deviceId, string action, int channelNumber, string result, int errorCode, string errorMessage)
        {
            if (_deviceClient == null || string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogDebug("IoT Hub client not initialized - command result not sent");
                return false;
            }

            try
            {
                var resultData = new
                {
                    deviceId,
                    action,
                    channelNumber,
                    result,
                    errorCode,
                    errorMessage,
                    timestamp = DateTimeOffset.UtcNow
                };

                var messageString = JsonSerializer.Serialize(resultData);

                var message = new Message(Encoding.UTF8.GetBytes(messageString))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8"
                };
                await _deviceClient.SendEventAsync(message);

                _logger.LogInformation("Command result sent to IoT Hub: {Action} on Ch{Channel}", action, channelNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send command result to IoT Hub");
                return false;
            }
        }

        // Direct Method Handlers
        private async Task<MethodResponse> HandleSetVoltsMethod(MethodRequest methodRequest, object userContext)
        {
            _logger.LogInformation("Direct Method 'SetVolts' invoked");

            try
            {
                var payload = Encoding.UTF8.GetString(methodRequest.Data);
                var command = JsonSerializer.Deserialize<CloudCommand>(payload, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (command == null || command.ChannelNumber < 0)
                {
                    return new MethodResponse(Encoding.UTF8.GetBytes("{\"status\":\"error\",\"message\":\"Invalid request - ChannelNumber required\"}"), 400);
                }

                if (_powerSupplyService == null)
                {
                    return new MethodResponse(Encoding.UTF8.GetBytes("{\"status\":\"error\",\"message\":\"PowerSupply service not initialized\"}"), 500);
                }

                var request = new SetVoltsRequest
                {
                    DeviceId = command.DeviceId ?? "ps-001",
                    ChannelNumber = command.ChannelNumber,
                    Volts = command.Volts
                };

                var response = await _powerSupplyService.SetVolts(request, null!);

                await SendCommandResultAsync(
                    request.DeviceId,
                    "setVolts",
                    command.ChannelNumber,
                    $"{command.Volts}V",
                    response.Result,
                    response.Message
                );

                if (response.Result == 0)
                {
                    var result = new 
                    { 
                        status = "success", 
                        channelNumber = command.ChannelNumber,
                        volts = command.Volts,
                        message = "Voltage set successfully"
                    };
                    return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)), 200);
                }
                else
                {
                    var error = new 
                    { 
                        status = "error", 
                        channelNumber = command.ChannelNumber,
                        message = response.Message 
                    };
                    return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), 400);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling SetVolts direct method");
                var error = new { status = "error", message = ex.Message };
                return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), 500);
            }
        }

        private async Task<MethodResponse> HandleSetAmpsMethod(MethodRequest methodRequest, object userContext)
        {
            _logger.LogInformation("Direct Method 'SetAmps' invoked");

            try
            {
                var payload = Encoding.UTF8.GetString(methodRequest.Data);
                var command = JsonSerializer.Deserialize<CloudCommand>(payload, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (command == null || command.ChannelNumber < 0)
                {
                    return new MethodResponse(Encoding.UTF8.GetBytes("{\"status\":\"error\",\"message\":\"Invalid request - ChannelNumber required\"}"), 400);
                }

                if (_powerSupplyService == null)
                {
                    return new MethodResponse(Encoding.UTF8.GetBytes("{\"status\":\"error\",\"message\":\"PowerSupply service not initialized\"}"), 500);
                }

                var request = new SetAmpsRequest
                {
                    DeviceId = command.DeviceId ?? "ps-001",
                    ChannelNumber = command.ChannelNumber,
                    Amps = command.Amps
                };

                var response = await _powerSupplyService.SetAmps(request, null!);

                await SendCommandResultAsync(
                    request.DeviceId,
                    "setAmps",
                    command.ChannelNumber,
                    $"{command.Amps}A",
                    response.Result,
                    response.Message
                );

                if (response.Result == 0)
                {
                    var result = new 
                    { 
                        status = "success", 
                        channelNumber = command.ChannelNumber,
                        amps = command.Amps,
                        message = "Current set successfully"
                    };
                    return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)), 200);
                }
                else
                {
                    var error = new 
                    { 
                        status = "error", 
                        channelNumber = command.ChannelNumber,
                        message = response.Message 
                    };
                    return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), 400);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling SetAmps direct method");
                var error = new { status = "error", message = ex.Message };
                return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), 500);
            }
        }

        private async Task<MethodResponse> HandleAllChannelsOnMethod(MethodRequest methodRequest, object userContext)
        {
            _logger.LogInformation("Direct Method 'AllChannelsOn' invoked");

            try
            {
                var payload = methodRequest.Data != null && methodRequest.Data.Length > 0
                    ? Encoding.UTF8.GetString(methodRequest.Data)
                    : "{}";

                var command = JsonSerializer.Deserialize<CloudCommand>(payload, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (_powerSupplyService == null)
                {
                    return new MethodResponse(Encoding.UTF8.GetBytes("{\"status\":\"error\",\"message\":\"PowerSupply service not initialized\"}"), 500);
                }

                var request = new AllChannelsOnRequest
                {
                    DeviceId = command?.DeviceId ?? "ps-001"
                };

                var response = await _powerSupplyService.AllChannelsOn(request, null!);

                await SendCommandResultAsync(
                    request.DeviceId,
                    "allChannelsOn",
                    -1,
                    "All channels ON",
                    response.Result,
                    response.Message
                );

                var result = new 
                { 
                    status = "success", 
                    message = "All channels turned on"
                };
                return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)), 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling AllChannelsOn direct method");
                var error = new { status = "error", message = ex.Message };
                return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), 500);
            }
        }

        private async Task<MethodResponse> HandleAllChannelsOffMethod(MethodRequest methodRequest, object userContext)
        {
            _logger.LogInformation("Direct Method 'AllChannelsOff' invoked");

            try
            {
                var payload = methodRequest.Data != null && methodRequest.Data.Length > 0
                    ? Encoding.UTF8.GetString(methodRequest.Data)
                    : "{}";

                var command = JsonSerializer.Deserialize<CloudCommand>(payload, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (_powerSupplyService == null)
                {
                    return new MethodResponse(Encoding.UTF8.GetBytes("{\"status\":\"error\",\"message\":\"PowerSupply service not initialized\"}"), 500);
                }

                var request = new AllChannelsOffRequest
                {
                    DeviceId = command?.DeviceId ?? "ps-001"
                };

                var response = await _powerSupplyService.AllChannelsOff(request, null!);

                await SendCommandResultAsync(
                    request.DeviceId,
                    "allChannelsOff",
                    -1,
                    "All channels OFF",
                    response.Result,
                    response.Message
                );

                var result = new 
                { 
                    status = "success", 
                    message = "All channels turned off"
                };
                return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)), 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling AllChannelsOff direct method");
                var error = new { status = "error", message = ex.Message };
                return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), 500);
            }
        }

        private async Task<MethodResponse> HandleGetDeviceStatusMethod(MethodRequest methodRequest, object userContext)
        {
            _logger.LogInformation("Direct Method 'GetDeviceStatus' invoked");

            try
            {
                var payload = methodRequest.Data != null && methodRequest.Data.Length > 0
                    ? Encoding.UTF8.GetString(methodRequest.Data)
                    : "{}";

                var command = JsonSerializer.Deserialize<CloudCommand>(payload, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (_powerSupplyService == null)
                {
                    return new MethodResponse(Encoding.UTF8.GetBytes("{\"status\":\"error\",\"message\":\"PowerSupply service not initialized\"}"), 500);
                }

                var request = new GetDeviceStatusRequest
                {
                    DeviceId = command?.DeviceId ?? "ps-001"
                };

                var response = await _powerSupplyService.GetDeviceStatus(request, null!);

                var result = new 
                { 
                    status = "success", 
                    isConnected = response.IsConnected,
                    numberOfChannels = response.NumberOfChannels,
                    deviceParameters = response.DeviceParameters
                };
                return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)), 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetDeviceStatus direct method");
                var error = new { status = "error", message = ex.Message };
                return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), 500);
            }
        }

        private async Task<MethodResponse> HandleGetChannelParametersMethod(MethodRequest methodRequest, object userContext)
        {
            _logger.LogInformation("Direct Method 'GetChannelParameters' invoked");

            try
            {
                var payload = Encoding.UTF8.GetString(methodRequest.Data);
                var command = JsonSerializer.Deserialize<CloudCommand>(payload, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (command == null || command.ChannelNumber < 0)
                {
                    return new MethodResponse(Encoding.UTF8.GetBytes("{\"status\":\"error\",\"message\":\"Invalid request - ChannelNumber required\"}"), 400);
                }

                if (_powerSupplyService == null)
                {
                    return new MethodResponse(Encoding.UTF8.GetBytes("{\"status\":\"error\",\"message\":\"PowerSupply service not initialized\"}"), 500);
                }

                var request = new GetChannelParametersRequest
                {
                    DeviceId = command.DeviceId ?? "ps-001",
                    ChannelNumber = command.ChannelNumber
                };

                var response = await _powerSupplyService.GetChannelParameters(request, null!);

                var result = new 
                { 
                    status = "success", 
                    channelNumber = command.ChannelNumber,
                    parameters = response.Parameters
                };
                return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)), 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetChannelParameters direct method");
                var error = new { status = "error", message = ex.Message };
                return new MethodResponse(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error)), 500);
            }
        }

        private Task<MethodResponse> HandleDefaultMethod(MethodRequest methodRequest, object userContext)
        {
            _logger.LogWarning("Unknown direct method called: {MethodName}", methodRequest.Name);

            var error = new { status = "error", message = $"Method '{methodRequest.Name}' not found" };
            var errorJson = JsonSerializer.Serialize(error);
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(errorJson), 404));
        }

        public void Dispose()
        {
            if (_deviceClient is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public class CloudCommand
    {
        public string Action { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public int ChannelNumber { get; set; }
        public double Volts { get; set; }
        public double Amps { get; set; }
    }
}
