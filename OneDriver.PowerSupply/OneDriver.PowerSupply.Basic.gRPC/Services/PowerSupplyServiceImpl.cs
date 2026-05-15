using Grpc.Core;
using OneDriver.PowerSupply.Basic;
using OneDriver.PowerSupply.Abstract.Contracts;
using OneDriver.PowerSupply.Abstract;
using OneDriver.PowerSupply.Abstract.Channels;
using System.Collections.Concurrent;
using OneDriver.PowerSupply.Basic.Channels;

namespace OneDriver.PowerSupply.Basic.gRPC.Services
{
    public class PowerSupplyServiceImpl : PowerSupplyService.PowerSupplyServiceBase
    {
        private readonly ILogger<PowerSupplyServiceImpl> _logger;
        private readonly ConcurrentDictionary<string, CommonDevice<CommonDeviceParams, CommonChannelParams, CommonProcessData>> _devices;

        public PowerSupplyServiceImpl(ILogger<PowerSupplyServiceImpl> logger)
        {
            _logger = logger;
            _devices = new ConcurrentDictionary<string, CommonDevice<CommonDeviceParams, CommonChannelParams, CommonProcessData>>();
        }

        public void RegisterDevice(string deviceId, CommonDevice<CommonDeviceParams, CommonChannelParams, CommonProcessData> device)
        {
            _devices.TryAdd(deviceId, device);
            _logger.LogInformation("Device {DeviceId} registered in service", deviceId);
        }

        public override Task<ConnectDeviceResponse> ConnectDevice(ConnectDeviceRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Connecting device: {DeviceName}, Product: {ProductType}, Port: {Port}",
                    request.DeviceName, request.ProductType, request.PortName);

                var deviceId = Guid.NewGuid().ToString();

                // TODO: Use Factory to create device based on product type
                // var device = PowerSupplyFactory.CreateDevice(request.DeviceName, request.ProductType, request.PortName);
                // _devices.TryAdd(deviceId, device);

                return Task.FromResult(new ConnectDeviceResponse
                {
                    Success = true,
                    Message = "Device connected successfully",
                    DeviceId = deviceId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect device");
                return Task.FromResult(new ConnectDeviceResponse
                {
                    Success = false,
                    Message = $"Failed to connect: {ex.Message}",
                    DeviceId = string.Empty
                });
            }
        }

        public override Task<DisconnectDeviceResponse> DisconnectDevice(DisconnectDeviceRequest request, ServerCallContext context)
        {
            try
            {
                if (_devices.TryRemove(request.DeviceId, out var device))
                {
                    // TODO: Properly dispose device
                    _logger.LogInformation("Device {DeviceId} disconnected", request.DeviceId);

                    return Task.FromResult(new DisconnectDeviceResponse
                    {
                        Success = true,
                        Message = "Device disconnected successfully"
                    });
                }

                return Task.FromResult(new DisconnectDeviceResponse
                {
                    Success = false,
                    Message = "Device not found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to disconnect device");
                return Task.FromResult(new DisconnectDeviceResponse
                {
                    Success = false,
                    Message = $"Failed to disconnect: {ex.Message}"
                });
            }
        }

        public override Task<GetDeviceStatusResponse> GetDeviceStatus(GetDeviceStatusRequest request, ServerCallContext context)
        {
            if (!_devices.TryGetValue(request.DeviceId, out var device))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Device not found"));
            }

            return Task.FromResult(new GetDeviceStatusResponse
            {
                IsConnected = true,
                NumberOfChannels = device.Elements.Count,
                DeviceParameters = new DeviceParameters
                {
                    Name = device.Parameters.Name,
                    MaxVolts = device.Parameters.MaxVolts,
                    MaxAmps = device.Parameters.MaxAmps,                    
                }
            });
        }

        public override Task<SetVoltsResponse> SetVolts(SetVoltsRequest request, ServerCallContext context)
        {
            try
            {
                if (!_devices.TryGetValue(request.DeviceId, out var device))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Device not found"));
                }

                var result = device.SetVolts(request.ChannelNumber, request.Volts);

                return Task.FromResult(new SetVoltsResponse
                {
                    Result = result,
                    Message = "Voltage set successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set volts");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override Task<SetAmpsResponse> SetAmps(SetAmpsRequest request, ServerCallContext context)
        {
            try
            {
                if (!_devices.TryGetValue(request.DeviceId, out var device))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Device not found"));
                }

                var result = device.SetAmps(request.ChannelNumber, request.Amps);

                return Task.FromResult(new SetAmpsResponse
                {
                    Result = result,
                    Message = "Current set successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set amps");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override Task<SetControlModeResponse> SetControlMode(SetControlModeRequest request, ServerCallContext context)
        {
            try
            {
                if (!_devices.TryGetValue(request.DeviceId, out var device))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Device not found"));
                }

                if (request.ChannelNumber < 0 || request.ChannelNumber >= device.Elements.Count)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid channel number"));
                }

                var mode = request.Mode switch
                {
                    ControlMode.Voltage => Definition.ControlMode.Voltage,
                    ControlMode.Current => Definition.ControlMode.Current,
                    _ => throw new ArgumentException("Invalid control mode")
                };

                device.Elements[request.ChannelNumber].Parameters.ControlMode = mode;

                return Task.FromResult(new SetControlModeResponse
                {
                    Success = true,
                    Message = "Control mode set successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set control mode");
                return Task.FromResult(new SetControlModeResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        public override Task<AllChannelsOnResponse> AllChannelsOn(AllChannelsOnRequest request, ServerCallContext context)
        {
            try
            {
                if (!_devices.TryGetValue(request.DeviceId, out var device))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Device not found"));
                }

                var result = device.AllChannelsOn();

                return Task.FromResult(new AllChannelsOnResponse
                {
                    Result = result,
                    Message = "All channels turned on"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to turn on all channels");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override Task<AllChannelsOffResponse> AllChannelsOff(AllChannelsOffRequest request, ServerCallContext context)
        {
            try
            {
                if (!_devices.TryGetValue(request.DeviceId, out var device))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Device not found"));
                }

                var result = device.AllChannelsOff();

                return Task.FromResult(new AllChannelsOffResponse
                {
                    Result = result,
                    Message = "All channels turned off"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to turn off all channels");
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task StreamProcessData(StreamProcessDataRequest request, IServerStreamWriter<ProcessDataUpdate> responseStream, ServerCallContext context)
        {
            if (!_devices.TryGetValue(request.DeviceId, out var device))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Device not found"));
            }

            try
            {
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    var channelsToStream = request.ChannelNumber == -1
                        ? Enumerable.Range(0, device.Elements.Count)
                        : new[] { request.ChannelNumber };

                    foreach (var channelNum in channelsToStream)
                    {
                        if (channelNum >= 0 && channelNum < device.Elements.Count)
                        {
                            var processData = (ChannelProcessData)device.Elements[channelNum].ProcessData;

                            await responseStream.WriteAsync(new ProcessDataUpdate
                            {
                                ChannelNumber = channelNum,
                                CurrentVolts = processData.Voltage,
                                CurrentAmps = processData.Current,
                                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            });
                        }
                    }

                    await Task.Delay(100, context.CancellationToken); // 10Hz update rate
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stream cancelled for device {DeviceId}", request.DeviceId);
            }
        }

        public override Task<GetChannelParametersResponse> GetChannelParameters(GetChannelParametersRequest request, ServerCallContext context)
        {
            if (!_devices.TryGetValue(request.DeviceId, out var device))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Device not found"));
            }

            if (request.ChannelNumber < 0 || request.ChannelNumber >= device.Elements.Count)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid channel number"));
            }

            var channel = device.Elements[request.ChannelNumber];

            return Task.FromResult(new GetChannelParametersResponse
            {
                Parameters = new ChannelParameters
                {
                    Name = channel.Parameters.Name,
                    DesiredVolts = channel.Parameters.DesiredVolts,
                    DesiredAmps = channel.Parameters.DesiredAmps,
                    ControlMode = channel.Parameters.ControlMode == Definition.ControlMode.Voltage 
                        ? ControlMode.Voltage 
                        : ControlMode.Current
                }
            });
        }

        public override Task<GetDeviceParametersResponse> GetDeviceParameters(GetDeviceParametersRequest request, ServerCallContext context)
        {
            if (!_devices.TryGetValue(request.DeviceId, out var device))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Device not found"));
            }

            return Task.FromResult(new GetDeviceParametersResponse
            {
                Parameters = new DeviceParameters
                {
                    Name = device.Parameters.Name,
                    MaxVolts = device.Parameters.MaxVolts,
                    MaxAmps = device.Parameters.MaxAmps,
                }
            });
        }
    }
}
