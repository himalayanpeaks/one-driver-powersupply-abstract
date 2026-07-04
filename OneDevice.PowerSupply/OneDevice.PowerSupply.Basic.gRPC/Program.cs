using OneDevice.PowerSupply.Basic.gRPC.Services;
using Grpc.Core;
using OneDevice.PowerSupply.Basic.gRPC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<AzureIoTHubService>();
builder.Services.AddSingleton<PowerSupplyServiceImpl>();
builder.Services.AddSingleton<CloudCommandHandler>();
builder.Services.AddHostedService<PowerSupplyHostedService>();
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGrpcService<PowerSupplyServiceImpl>();

app.MapGet("/api/powersupply/state", (string? deviceId, PowerSupplyServiceImpl powerSupplyService, IConfiguration configuration) =>
{
	var resolvedDeviceId = ResolveDeviceId(deviceId, powerSupplyService, configuration);
	if (resolvedDeviceId is null)
	{
		return Results.NotFound(new { error = "No registered power supply device found." });
	}

	var snapshot = powerSupplyService.GetDeviceSnapshot(resolvedDeviceId);
	if (snapshot is null)
	{
		return Results.NotFound(new { error = "Device not found.", deviceId = resolvedDeviceId });
	}

	return Results.Ok(snapshot);
});

app.MapPost("/api/powersupply/channel/{channelNumber:int}/set", async (
	int channelNumber,
	SetChannelRequest request,
	PowerSupplyServiceImpl powerSupplyService,
	IConfiguration configuration) =>
{
	var resolvedDeviceId = ResolveDeviceId(request.DeviceId, powerSupplyService, configuration);
	if (resolvedDeviceId is null)
	{
		return Results.NotFound(new { error = "No registered power supply device found." });
	}

	try
	{
		if (request.Volts.HasValue)
		{
			var voltsResponse = await powerSupplyService.SetVolts(new SetVoltsRequest
			{
				DeviceId = resolvedDeviceId,
				ChannelNumber = channelNumber,
				Volts = request.Volts.Value
			}, null!);

			if (voltsResponse.Result != 0)
			{
				return Results.BadRequest(new { error = voltsResponse.Message, result = voltsResponse.Result });
			}
		}

		if (request.Amps.HasValue)
		{
			var ampsResponse = await powerSupplyService.SetAmps(new SetAmpsRequest
			{
				DeviceId = resolvedDeviceId,
				ChannelNumber = channelNumber,
				Amps = request.Amps.Value
			}, null!);

			if (ampsResponse.Result != 0)
			{
				return Results.BadRequest(new { error = ampsResponse.Message, result = ampsResponse.Result });
			}
		}

		if (!string.IsNullOrWhiteSpace(request.Mode))
		{
			var parsedMode = request.Mode.Trim().Equals("current", StringComparison.OrdinalIgnoreCase)
				? OneDevice.PowerSupply.Basic.gRPC.ControlMode.Current
				: OneDevice.PowerSupply.Basic.gRPC.ControlMode.Voltage;

			var modeResponse = await powerSupplyService.SetControlMode(new SetControlModeRequest
			{
				DeviceId = resolvedDeviceId,
				ChannelNumber = channelNumber,
				Mode = parsedMode
			}, null!);

			if (!modeResponse.Success)
			{
				return Results.BadRequest(new { error = modeResponse.Message });
			}
		}

		var snapshot = powerSupplyService.GetDeviceSnapshot(resolvedDeviceId);
		return Results.Ok(snapshot);
	}
	catch (RpcException ex)
	{
		return Results.BadRequest(new { error = ex.Status.Detail });
	}
});

app.MapPost("/api/powersupply/all/on", async (DeviceActionRequest request, PowerSupplyServiceImpl powerSupplyService, IConfiguration configuration) =>
{
	var resolvedDeviceId = ResolveDeviceId(request.DeviceId, powerSupplyService, configuration);
	if (resolvedDeviceId is null)
	{
		return Results.NotFound(new { error = "No registered power supply device found." });
	}

	try
	{
		var response = await powerSupplyService.AllChannelsOn(new AllChannelsOnRequest { DeviceId = resolvedDeviceId }, null!);
		if (response.Result != 0)
		{
			return Results.BadRequest(new { error = response.Message, result = response.Result });
		}

		return Results.Ok(new { success = true, message = response.Message });
	}
	catch (RpcException ex)
	{
		return Results.BadRequest(new { error = ex.Status.Detail });
	}
});

app.MapPost("/api/powersupply/all/off", async (DeviceActionRequest request, PowerSupplyServiceImpl powerSupplyService, IConfiguration configuration) =>
{
	var resolvedDeviceId = ResolveDeviceId(request.DeviceId, powerSupplyService, configuration);
	if (resolvedDeviceId is null)
	{
		return Results.NotFound(new { error = "No registered power supply device found." });
	}

	try
	{
		var response = await powerSupplyService.AllChannelsOff(new AllChannelsOffRequest { DeviceId = resolvedDeviceId }, null!);
		if (response.Result != 0)
		{
			return Results.BadRequest(new { error = response.Message, result = response.Result });
		}

		return Results.Ok(new { success = true, message = response.Message });
	}
	catch (RpcException ex)
	{
		return Results.BadRequest(new { error = ex.Status.Detail });
	}
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Logger.LogInformation("Starting OneDevice.PowerSupply.Basic.gRPC service");

app.Run();

static string? ResolveDeviceId(string? requestedDeviceId, PowerSupplyServiceImpl powerSupplyService, IConfiguration configuration)
{
	if (!string.IsNullOrWhiteSpace(requestedDeviceId) && powerSupplyService.HasDevice(requestedDeviceId))
	{
		return requestedDeviceId;
	}

	var configuredDeviceId = configuration["PowerSupply:DefaultDeviceId"];
	if (!string.IsNullOrWhiteSpace(configuredDeviceId) && powerSupplyService.HasDevice(configuredDeviceId))
	{
		return configuredDeviceId;
	}

	return powerSupplyService.GetFirstDeviceId();
}

public sealed record SetChannelRequest(string? DeviceId, double? Volts, double? Amps, string? Mode);
public sealed record DeviceActionRequest(string? DeviceId);
