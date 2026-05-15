# OneDriver.PowerSupply.Basic.gRPC

A gRPC service for remote control and monitoring of power supply devices through Azure IoT Hub cloud access.

## Overview

This project provides Azure IoT Hub integration to control Power Supply devices remotely from the cloud. The service automatically connects to the device on startup and listens for commands from Azure IoT Hub.

## Features

- **Auto-Connect**: Automatically connects to Power Supply at startup (configurable)
- **Azure IoT Hub Integration**: Send commands from Azure cloud via:
  - Cloud-to-Device (C2D) messages
  - Direct Methods
- **Real-time Telemetry**: Sends voltage and current measurements to Azure
- **Cloud-Ready**: Designed for cloud deployment and remote access
- **gRPC Protocol**: High-performance, cross-platform communication

## Configuration

### appsettings.json

```json
{
  "AzureIoTHub": {
    "ConnectionString": "HostName=xxx.azure-devices.net;DeviceId=xxx;SharedAccessKey=xxx"
  },
  "PowerSupply": {
    "DefaultDeviceId": "ps-001",
    "ComPort": "COM5",
    "AutoConnectOnStartup": true,
    "ProductType": "KD3005P",
    "DeviceName": "PowerSupply1"
  }
}
```

- **ComPort**: Default is `COM5` for Power Supply (vs. `COM3` for IoLink Master)
- **AutoConnectOnStartup**: Set to `true` to auto-connect on service start
- **ProductType**: Currently supports `KD3005P`

## Azure IoT Hub Commands

### Direct Methods (Recommended)

Direct Methods provide immediate response from the device.

#### SetVolts
```json
{
  "deviceId": "ps-001",
  "channelNumber": 0,
  "volts": 12.5
}
```

#### SetAmps
```json
{
  "deviceId": "ps-001",
  "channelNumber": 0,
  "amps": 2.0
}
```

#### AllChannelsOn
```json
{
  "deviceId": "ps-001"
}
```

#### AllChannelsOff
```json
{
  "deviceId": "ps-001"
}
```

#### GetDeviceStatus
```json
{
  "deviceId": "ps-001"
}
```

#### GetChannelParameters
```json
{
  "deviceId": "ps-001",
  "channelNumber": 0
}
```

### Cloud-to-Device Messages

Send JSON messages to the device:

```json
{
  "action": "setVolts",
  "deviceId": "ps-001",
  "channelNumber": 0,
  "volts": 12.5
}
```

Supported actions:
- `setVolts` / `setVoltage`
- `setAmps` / `setCurrent`
- `allChannelsOn` / `allOn`
- `allChannelsOff` / `allOff`
- `getStatus` / `status`

## Telemetry

The service automatically sends telemetry to Azure IoT Hub:

```json
{
  "deviceId": "ps-001",
  "channelNumber": 0,
  "volts": 12.5,
  "amps": 1.8,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## Usage

### Starting the Service

```bash
dotnet run --project OneDriver.PowerSupply.Basic.gRPC
```

The service will:
1. Connect to Power Supply at configured COM port
2. Register with Azure IoT Hub
3. Start listening for cloud commands
4. Send initial telemetry

### Sending Commands from Azure Portal

1. Go to Azure IoT Hub > Devices > Your Device
2. Click "Direct Method"
3. Enter method name (e.g., `SetVolts`)
4. Enter payload JSON
5. Click "Invoke Method"

### Sending Commands from Azure CLI

```bash
# Direct Method
az iot hub invoke-device-method \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --method-name SetVolts \
  --method-payload '{"channelNumber":0,"volts":12.5}'

# Cloud-to-Device Message
az iot device c2d-message send \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --data '{"action":"setVolts","channelNumber":0,"volts":12.5}'
```

## gRPC Service Endpoints

The service also exposes gRPC endpoints for direct client access:

- `ConnectDevice` - Manual device connection
- `DisconnectDevice` - Disconnect from device
- `SetVolts` - Set channel voltage
- `SetAmps` - Set channel current
- `AllChannelsOn` / `AllChannelsOff` - Control all channels
- `StreamProcessData` - Stream real-time data
- `GetDeviceStatus` / `GetChannelParameters` - Query device state

## Logging

Logs show:
- Device connection status
- Azure IoT Hub connection
- Commands received and executed
- Telemetry sent
- Errors and warnings

## Architecture Differences from IoLink.gRPC

| Feature | IoLink.gRPC | PowerSupply.gRPC |
|---------|-------------|------------------|
| Default COM Port | COM3 | COM5 |
| Device Type | IO-Link Master | Power Supply |
| Product | TMG Master 2 | KD3005P |
| IODD Finder | Required | Not used |
| Channels | Sensor ports | Power channels |
| Commands | ReadParameter, WriteParameter | SetVolts, SetAmps |

## Security Considerations

- Store Azure connection string securely (use Azure Key Vault in production)
- Enable authentication on gRPC endpoints for production
- Use TLS/SSL for encrypted communication
- Implement rate limiting to prevent abuse

## Deployment

For cloud deployment:
1. Update Azure IoT Hub connection string
2. Configure COM port for your environment
3. Deploy as a Windows Service or Azure IoT Edge module
4. Ensure serial port access permissions
