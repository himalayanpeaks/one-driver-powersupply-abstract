using OneDriver.PowerSupply.Basic.gRPC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<AzureIoTHubService>();
builder.Services.AddSingleton<PowerSupplyServiceImpl>();
builder.Services.AddSingleton<CloudCommandHandler>();
builder.Services.AddHostedService<PowerSupplyHostedService>();
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<PowerSupplyServiceImpl>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Logger.LogInformation("Starting OneDriver.PowerSupply.Basic.gRPC service");

app.Run();
