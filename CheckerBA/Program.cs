using System.Text;
using CheckerBA.Application.Services;
using CheckerBA.Domain.Interfaces;
using CheckerBA.Hubs;
using CheckerBA.Infrastructure.MongoDB;
using CheckerBA.Infrastructure.Mqtt;
using CheckerBA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── MongoDB ───────────────────────────────────────────────────────────────
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbContext>();

// ── Repositories ─────────────────────────────────────────────────────────
builder.Services.AddSingleton<IDeviceRepository, MongoDeviceRepository>();
builder.Services.AddSingleton<ITelemetryRepository, MongoTelemetryRepository>();
builder.Services.AddSingleton<IEventRepository, MongoEventRepository>();
builder.Services.AddSingleton<IEnergyUsedRepository, MongoEnergyUsedRepository>();
builder.Services.AddSingleton<IUserRepository, MongoUserRepository>();

// ── Application Services ──────────────────────────────────────────────────
builder.Services.AddSingleton<DeviceManagementService>();
builder.Services.AddSingleton<TelemetryProcessingService>();
builder.Services.AddSingleton<EventProcessingService>();
builder.Services.AddSingleton<AuthService>();

// ── MQTT ──────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IMqttService, MqttClientService>();
builder.Services.AddHostedService<MqttListenerService>();

// ── SignalR ───────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── JWT Authentication ────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // Cho phép SignalR gửi token qua query string
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) &&
                    (ctx.HttpContext.Request.Path.StartsWithSegments("/hubs")))
                {
                    ctx.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── Controllers + Swagger ─────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EquipmentChecker API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token. Ví dụ: Bearer eyJhbGci..."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS (cho Web/WPF/MAUI client) ───────────────────────────────────────
builder.Services.AddCors(opt => opt.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DeviceHub>("/hubs/device");
app.MapGet("/", () => "EquipmentChecker API is running");

app.Run();
