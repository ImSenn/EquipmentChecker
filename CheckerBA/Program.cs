using CheckerBA.Domain.Interfaces;
using CheckerBA.Infrastructure.MongoDB;
using CheckerBA.Infrastructure.Repositories;
using CheckerBA.Infrastructure.Mqtt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<IDeviceRepository, MongoDeviceRepository>();
builder.Services.AddSingleton<IMqttService, MqttClientService>();
builder.Services.AddHostedService<MqttListenerService>();

var app = builder.Build();

app.MapGet("/", () => "EquipmentChecker API is running");

app.Run();

