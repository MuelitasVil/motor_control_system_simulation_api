using MotorControlSystemSimulationApi.Repositories.InMemory;
using MotorControlSystemSimulationApi.Services;
using MotorControlSystemSimulationApi.Hubs;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSignalR().AddJsonProtocol(options =>
    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddInMemoryRepositories();
builder.Services.AddSingleton<MotorSimulationService>();
builder.Services.AddSingleton<VehicleCreationService>();
builder.Services.AddSingleton<MotorCreationService>();
builder.Services.AddSingleton<MotorAssociationCreationService>();
builder.Services.AddSingleton<TripManagementService>();
builder.Services.AddSingleton<VehicleMaintenanceService>();
builder.Services.AddHostedService<MotorTelemetryWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Motor Control API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<MotorTelemetryHub>("/hubs/motor-telemetry");

app.Run();
