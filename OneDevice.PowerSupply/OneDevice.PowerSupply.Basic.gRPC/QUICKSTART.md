# OneDevice.PowerSupply.Basic.gRPC - Quick Start

## What is this?

A cloud-enabled gRPC service that automatically connects to your Power Supply device and allows you to control it from Azure IoT Hub.

## Key Features

✅ **Auto-Connect**: Service connects to Power Supply at COM5 on startup  
✅ **Azure Integration**: Control via Azure IoT Hub Direct Methods and C2D messages  
✅ **Real-time Telemetry**: Sends voltage/current data to Azure  
✅ **Cloud-Ready**: Send commands from anywhere via Azure  
✅ **Similar to IoLink.gRPC**: Same architecture, different device type  

## Quick Setup (5 minutes)

### 1. Configure appsettings.json

```json
{
  "AzureIoTHub": {
    "ConnectionString": "HostName=xxx.azure-devices.net;DeviceId=xxx;SharedAccessKey=xxx"
  },
  "PowerSupply": {
    "ComPort": "COM5",
    "AutoConnectOnStartup": true
  }
}
```

### 2. Run the Service

```bash
dotnet run
```

Expected output:
```
info: Successfully connected to Power Supply at COM5
info: Started listening for Direct Methods from Azure
info: PowerSupply ps-001 ready to receive commands from Azure
```

### 3. Send Command from Azure

**Azure Portal > IoT Hub > Devices > powersupply-001 > Direct Method**

Method: `SetVolts`  
Payload: `{"channelNumber":0,"volts":12.5}`

OR

**Azure CLI:**
```bash
az iot hub invoke-device-method \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --method-name SetVolts \
  --method-payload '{"channelNumber":0,"volts":12.5}'
```

## Architecture

```
Azure IoT Hub (Cloud)
     ↓ Direct Methods / C2D Messages
     ↓
PowerSupply.Basic.gRPC Service (PC)
     ↓ Serial Communication (COM5)
     ↓
Power Supply Device (KD3005P)
```

## Available Commands

| Command | Description | Example |
|---------|-------------|---------|
| SetVolts | Set channel voltage | `{"channelNumber":0,"volts":12.5}` |
| SetAmps | Set channel current | `{"channelNumber":0,"amps":2.0}` |
| AllChannelsOn | Turn on all channels | `{"deviceId":"ps-001"}` |
| AllChannelsOff | Turn off all channels | `{"deviceId":"ps-001"}` |
| GetDeviceStatus | Get device info | `{"deviceId":"ps-001"}` |
| GetChannelParameters | Get channel state | `{"channelNumber":0}` |

## Configuration Differences: IoLink vs PowerSupply

| Setting | IoLink.gRPC | PowerSupply.gRPC |
|---------|-------------|------------------|
| COM Port | COM3 | COM5 |
| Device Type | IO-Link Master | Power Supply |
| Product | TMG Master 2 | KD3005P |
| Commands | ReadParameter, WriteParameter | SetVolts, SetAmps |
| Config Section | `IoLinkMaster` | `PowerSupply` |

## File Structure

```
OneDevice.PowerSupply.Basic.gRPC/
├── Services/
│   ├── PowerSupplyServiceImpl.cs        # gRPC service implementation
│   ├── AzureIoTHubService.cs           # Azure IoT Hub integration
│   ├── CloudCommandHandler.cs           # C2D message handler
│   └── PowerSupplyHostedService.cs      # Auto-connect on startup
├── Protos/
│   └── powersupply_basic.proto          # gRPC service definition
├── Program.cs                            # ASP.NET Core host
├── appsettings.json                      # Configuration
├── README.md                             # Full documentation
├── CLOUD_COMMANDS.md                     # Azure command examples
└── AZURE_SETUP.md                        # Azure setup guide
```

## Documentation

- **README.md** - Complete service documentation
- **AZURE_SETUP.md** - Step-by-step Azure configuration
- **CLOUD_COMMANDS.md** - Azure command examples (CLI, Portal, Python, C#)

## Troubleshooting

**Device won't connect:**
- Check COM5 is correct port
- Verify Power Supply is plugged in
- Ensure no other app is using the port

**Azure commands not working:**
- Verify connection string in appsettings.json
- Check device is registered in Azure IoT Hub
- Look for errors in service logs

**No telemetry in Azure:**
- Ensure service started successfully
- Monitor events: `az iot hub monitor-events --hub-name xxx`
- Check Azure free tier limits (8,000 msg/day)

## Next Steps

1. ✅ Service is running
2. 🔲 Test commands from Azure Portal
3. 🔲 Monitor telemetry in Azure
4. 🔲 Set up alerts/monitoring
5. 🔲 Deploy as Windows Service (production)

## Need Help?

- Check logs for detailed error messages
- Review AZURE_SETUP.md for Azure configuration
- Review CLOUD_COMMANDS.md for command examples
- Ensure Power Supply is on and connected to COM5
