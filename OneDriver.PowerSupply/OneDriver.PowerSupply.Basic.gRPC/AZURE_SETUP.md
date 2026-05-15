# Azure IoT Hub Setup Guide

Step-by-step guide to configure Azure IoT Hub for the Power Supply gRPC service.

## 1. Create Azure IoT Hub

### Using Azure Portal

1. Go to [Azure Portal](https://portal.azure.com)
2. Click **Create a resource**
3. Search for **IoT Hub**
4. Click **Create**
5. Fill in the details:
   - **Subscription**: Your subscription
   - **Resource Group**: Create new or select existing
   - **IoT Hub Name**: `iot-powersupply-5678` (must be globally unique)
   - **Region**: Choose nearest region
   - **Tier**: F1 (Free) or S1 (Standard)
6. Click **Review + Create** then **Create**

### Using Azure CLI

```bash
# Create resource group
az group create --name rg-powersupply --location eastus

# Create IoT Hub (Free tier)
az iot hub create \
  --name iot-powersupply-5678 \
  --resource-group rg-powersupply \
  --sku F1 \
  --location eastus
```

## 2. Register Device

### Using Azure Portal

1. In your IoT Hub, go to **Device management > Devices**
2. Click **+ Add Device**
3. Enter Device ID: `powersupply-001`
4. Authentication type: **Symmetric key** (default)
5. Auto-generate keys: **Enabled**
6. Click **Save**

### Using Azure CLI

```bash
az iot hub device-identity create \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678
```

## 3. Get Connection String

### Using Azure Portal

1. Go to **Device management > Devices**
2. Click on **powersupply-001**
3. Copy **Primary Connection String**

   It looks like:
   ```
   HostName=iot-powersupply-5678.azure-devices.net;DeviceId=powersupply-001;SharedAccessKey=abc123...
   ```

### Using Azure CLI

```bash
az iot hub device-identity connection-string show \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678
```

## 4. Configure appsettings.json

Update your `appsettings.json`:

```json
{
  "AzureIoTHub": {
    "ConnectionString": "HostName=iot-powersupply-5678.azure-devices.net;DeviceId=powersupply-001;SharedAccessKey=YOUR_KEY_HERE"
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

⚠️ **Security**: Never commit connection strings to source control!

## 5. Test Connection

### Start the Service

```bash
cd OneDriver.PowerSupply.Basic.gRPC
dotnet run
```

Look for these log messages:
```
info: Azure IoT Hub client initialized successfully
info: Successfully connected to Power Supply at COM5
info: Started listening for Cloud-to-Device messages from Azure
info: Started listening for Direct Methods from Azure
```

### Monitor Device in Azure Portal

1. Go to your IoT Hub
2. Navigate to **Device management > Devices**
3. Click on **powersupply-001**
4. Check **Connection state**: Should show **Connected**

## 6. Send Test Command

### Using Azure Portal

1. In device details, click **Direct Method**
2. Method name: `GetDeviceStatus`
3. Payload:
   ```json
   {
     "deviceId": "ps-001"
   }
   ```
4. Click **Invoke Method**

Expected response:
```json
{
  "status": "success",
  "isConnected": true,
  "numberOfChannels": 1,
  "deviceParameters": {
    "name": "PowerSupply1",
    "maxVolts": 30.0,
    "maxAmps": 5.0
  }
}
```

### Using Azure CLI

```bash
az iot hub invoke-device-method \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --method-name GetDeviceStatus \
  --method-payload '{"deviceId":"ps-001"}'
```

## 7. Monitor Telemetry

### Using Azure CLI

```bash
az iot hub monitor-events \
  --hub-name iot-powersupply-5678 \
  --device-id powersupply-001
```

You should see telemetry messages:
```json
{
  "deviceId": "ps-001",
  "channelNumber": 0,
  "volts": 0.0,
  "amps": 0.0,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Using Azure Portal (IoT Hub Metrics)

1. Go to **Monitoring > Metrics**
2. Add metric: **Telemetry messages sent**
3. Add metric: **C2D messages completed**
4. Add metric: **Direct methods succeeded**

## 8. Optional: Set up Message Routing

Route telemetry to storage or Event Hub:

1. Go to **Hub settings > Message routing**
2. Click **+ Add**
3. Configure route:
   - **Name**: `telemetry-to-storage`
   - **Endpoint**: Create new (Storage, Event Hub, etc.)
   - **Data source**: Device Telemetry Messages
   - **Routing query**: `true` (route all)
4. Click **Save**

## Security Best Practices

### 1. Use Azure Key Vault

Store connection string in Key Vault:

```bash
# Create Key Vault
az keyvault create \
  --name kv-powersupply \
  --resource-group rg-powersupply \
  --location eastus

# Store connection string
az keyvault secret set \
  --vault-name kv-powersupply \
  --name iot-connection-string \
  --value "HostName=..."
```

Update your code to read from Key Vault:
```csharp
var secretClient = new SecretClient(new Uri("https://kv-powersupply.vault.azure.net/"), new DefaultAzureCredential());
var secret = await secretClient.GetSecretAsync("iot-connection-string");
var connectionString = secret.Value.Value;
```

### 2. Use Managed Identity

For Azure-hosted services (App Service, Container Instance):

1. Enable System-Assigned Managed Identity
2. Grant identity access to IoT Hub
3. Use token-based authentication instead of connection string

### 3. Rotate Keys Regularly

```bash
# Regenerate primary key
az iot hub device-identity renew-key \
  --device-id powersupply-001 \
  --hub-name iot-powersupply-5678 \
  --key-type primary
```

## Troubleshooting

### "Connection refused" or "Unauthorized"
- Verify connection string is correct
- Check device is registered in IoT Hub
- Ensure SharedAccessKey matches

### "Device not found"
- Verify device ID in connection string matches registered device
- Check IoT Hub name is correct

### "Quota exceeded"
- Free tier limited to 8,000 messages/day
- Upgrade to S1 tier or reduce telemetry frequency

### No telemetry received
- Check device is connected (Azure Portal > Devices)
- Monitor service logs for errors
- Verify firewall allows outbound MQTT (port 8883)

## Cost Optimization

### Free Tier Limits (F1)
- 8,000 messages/day
- 1 IoT Hub per subscription
- No device provisioning service

### Reduce Costs
1. Reduce telemetry frequency
2. Use batch sending
3. Filter messages before routing
4. Use free tier for development

## Next Steps

1. ✅ Set up IoT Hub
2. ✅ Register device
3. ✅ Configure service
4. ✅ Test commands
5. 🔲 Set up monitoring/alerts
6. 🔲 Configure message routing
7. 🔲 Implement security best practices
8. 🔲 Deploy to production environment
