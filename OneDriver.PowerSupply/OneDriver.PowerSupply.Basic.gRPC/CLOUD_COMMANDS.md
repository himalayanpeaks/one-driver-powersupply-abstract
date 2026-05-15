# Azure IoT Hub Cloud Commands for Power Supply

This document provides examples for sending commands to the Power Supply device through Azure IoT Hub.

## Prerequisites

1. Azure IoT Hub created
2. Device registered in IoT Hub
3. Connection string configured in `appsettings.json`
4. Service running and connected to Power Supply

## Azure Portal - Direct Methods

Navigate to: **Azure Portal > IoT Hub > Devices > [Your Device] > Direct Method**

### 1. Set Voltage

**Method name:** `SetVolts`

**Payload:**
```json
{
  "deviceId": "ps-001",
  "channelNumber": 0,
  "volts": 12.5
}
```

**Response:**
```json
{
  "status": "success",
  "channelNumber": 0,
  "volts": 12.5,
  "message": "Voltage set successfully"
}
```

### 2. Set Current

**Method name:** `SetAmps`

**Payload:**
```json
{
  "deviceId": "ps-001",
  "channelNumber": 0,
  "amps": 2.0
}
```

### 3. Turn On All Channels

**Method name:** `AllChannelsOn`

**Payload:**
```json
{
  "deviceId": "ps-001"
}
```

### 4. Turn Off All Channels

**Method name:** `AllChannelsOff`

**Payload:**
```json
{
  "deviceId": "ps-001"
}
```

### 5. Get Device Status

**Method name:** `GetDeviceStatus`

**Payload:**
```json
{
  "deviceId": "ps-001"
}
```

**Response:**
```json
{
  "status": "success",
  "isConnected": true,
  "numberOfChannels": 1,
  "deviceParameters": {
    "name": "PowerSupply1",
    "maxVolts": 30.0,
    "maxAmps": 5.0,
    "minVolts": 0.0,
    "minAmps": 0.0
  }
}
```

### 6. Get Channel Parameters

**Method name:** `GetChannelParameters`

**Payload:**
```json
{
  "deviceId": "ps-001",
  "channelNumber": 0
}
```

**Response:**
```json
{
  "status": "success",
  "channelNumber": 0,
  "parameters": {
    "name": "Ch0",
    "desiredVolts": 12.5,
    "desiredAmps": 2.0,
    "controlMode": "CONTROL_MODE_VOLTAGE"
  }
}
```

## Azure CLI Commands

### Install Azure CLI
```bash
# Windows
winget install -e --id Microsoft.AzureCLI

# Or download from https://aka.ms/installazurecliwindows
```

### Login
```bash
az login
```

### Direct Method Examples

```bash
# Set Voltage
az iot hub invoke-device-method \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --method-name SetVolts \
  --method-payload '{"deviceId":"ps-001","channelNumber":0,"volts":12.5}'

# Set Current
az iot hub invoke-device-method \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --method-name SetAmps \
  --method-payload '{"deviceId":"ps-001","channelNumber":0,"amps":2.0}'

# All Channels ON
az iot hub invoke-device-method \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --method-name AllChannelsOn \
  --method-payload '{"deviceId":"ps-001"}'

# All Channels OFF
az iot hub invoke-device-method \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --method-name AllChannelsOff \
  --method-payload '{"deviceId":"ps-001"}'

# Get Status
az iot hub invoke-device-method \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --method-name GetDeviceStatus \
  --method-payload '{"deviceId":"ps-001"}'
```

### Cloud-to-Device (C2D) Messages

```bash
# Set Voltage via C2D message
az iot device c2d-message send \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --data '{"action":"setVolts","deviceId":"ps-001","channelNumber":0,"volts":12.5}'

# Set Current via C2D message
az iot device c2d-message send \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --data '{"action":"setAmps","deviceId":"ps-001","channelNumber":0,"amps":2.0}'

# Turn on all channels
az iot device c2d-message send \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --data '{"action":"allChannelsOn","deviceId":"ps-001"}'
```

## Python Example

```python
from azure.iot.hub import IoTHubRegistryManager
import json

# Configuration
CONNECTION_STRING = "HostName=xxx.azure-devices.net;SharedAccessKeyName=xxx;SharedAccessKey=xxx"
DEVICE_ID = "powersupply-001"

# Create registry manager
registry_manager = IoTHubRegistryManager(CONNECTION_STRING)

# Direct Method - Set Voltage
method_name = "SetVolts"
payload = {
    "deviceId": "ps-001",
    "channelNumber": 0,
    "volts": 12.5
}

response = registry_manager.invoke_device_method(
    DEVICE_ID,
    method_name,
    json.dumps(payload)
)

print(f"Status: {response.status}")
print(f"Payload: {response.payload}")

# C2D Message - Set Current
message = {
    "action": "setAmps",
    "deviceId": "ps-001",
    "channelNumber": 0,
    "amps": 2.0
}

registry_manager.send_c2d_message(DEVICE_ID, json.dumps(message))
print("C2D message sent")
```

## C# Example

```csharp
using Microsoft.Azure.Devices;
using System.Text;
using System.Text.Json;

var connectionString = "HostName=xxx.azure-devices.net;SharedAccessKeyName=xxx;SharedAccessKey=xxx";
var deviceId = "powersupply-001";

var serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

// Direct Method - Set Voltage
var methodInvocation = new CloudToDeviceMethod("SetVolts")
{
    ResponseTimeout = TimeSpan.FromSeconds(30)
};

var payload = new 
{
    deviceId = "ps-001",
    channelNumber = 0,
    volts = 12.5
};

methodInvocation.SetPayloadJson(JsonSerializer.Serialize(payload));

var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);

Console.WriteLine($"Status: {response.Status}");
Console.WriteLine($"Result: {response.GetPayloadAsJson()}");

// C2D Message - Turn on all channels
var message = new 
{
    action = "allChannelsOn",
    deviceId = "ps-001"
};

var c2dMessage = new Message(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)));
await serviceClient.SendAsync(deviceId, c2dMessage);

Console.WriteLine("C2D message sent");
```

## Monitoring Telemetry

### Azure CLI - Monitor Events
```bash
az iot hub monitor-events \
  --hub-name iot-powersupply-5678 \
  --device-id powersupply-001
```

### Expected Telemetry Output
```json
{
  "event": {
    "origin": "powersupply-001",
    "module": "",
    "interface": "",
    "component": "",
    "payload": {
      "deviceId": "ps-001",
      "channelNumber": 0,
      "volts": 12.5,
      "amps": 1.8,
      "timestamp": "2024-01-15T10:30:00.000Z"
    }
  }
}
```

## Testing Sequence

1. **Start the service**
   ```bash
   dotnet run --project OneDriver.PowerSupply.Basic.gRPC
   ```

2. **Verify connection** (check logs for "Successfully connected to Power Supply at COM5")

3. **Send Direct Method** via Azure Portal or CLI

4. **Check logs** for command execution

5. **Monitor telemetry** to see updated values

## Troubleshooting

### Device Not Responding
- Check service is running
- Verify COM port is correct in appsettings.json
- Check Azure connection string is valid
- Review service logs for errors

### Authentication Errors
- Verify Azure IoT Hub connection string
- Check device is registered in IoT Hub
- Ensure device ID matches configuration

### Command Errors
- Verify JSON payload format
- Check channel number is valid (0-based index)
- Ensure voltage/current within device limits
- Review direct method response for error details
