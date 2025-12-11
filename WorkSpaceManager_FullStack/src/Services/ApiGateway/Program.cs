using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// JWT Authentication
var keycloakAuthority = builder.Configuration["Keycloak:Authority"];
var keycloakRealm = builder.Configuration["Keycloak:Realm"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = $"{keycloakAuthority}/realms/{keycloakRealm}";
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = false; // Set to true in production
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[API Gateway] Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirst("sub")?.Value;
                var tenantId = context.Principal?.FindFirst("tenant_id")?.Value;
                Console.WriteLine($"[API Gateway] Token validated - User: {userId}, Tenant: {tenantId}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"[API Gateway] Authentication challenge: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "https://localhost:3000",
                "https://localhost:3001"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add Ocelot
builder.Services.AddOcelot();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Configure middleware pipeline
app.UseCors("AllowAll");

// Custom middleware for logging
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var method = context.Request.Method;
    Console.WriteLine($"[API Gateway] {method} {path}");
    
    await next();
    
    Console.WriteLine($"[API Gateway] {method} {path} - Status: {context.Response.StatusCode}");
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "API Gateway",
    timestamp = DateTime.UtcNow
}));

// Gateway info endpoint
app.MapGet("/", () => Results.Ok(new
{
    service = "WorkSpace Manager API Gateway",
    version = "1.0.0",
    endpoints = new
    {
        bookings = "/api/bookings",
        buildings = "/api/buildings",
        floors = "/api/floors",
        desks = "/api/desks",
        meetingRooms = "/api/meetingrooms",
        health = new
        {
            gateway = "/health",
            bookingService = "/health/booking",
            spaceService = "/health/space"
        }
    },
    documentation = "Refer to individual service Swagger documentation"
}));

// Use Ocelot
await app.UseOcelot();

Console.WriteLine("API Gateway started successfully");
Console.WriteLine($"Listening on: {builder.Configuration["Urls"] ?? "http://localhost:5000"}");

app.Run();
