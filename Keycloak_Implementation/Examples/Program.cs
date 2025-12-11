using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WorkSpaceManager.Identity.Authorization;
using WorkSpaceManager.Identity.Data;
using WorkSpaceManager.Identity.Middleware;
using WorkSpaceManager.Identity.Models;
using WorkSpaceManager.Identity.Services;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// 1. CONFIGURATION
// =============================================

// Configure Keycloak options from appsettings.json
builder.Services.Configure<KeycloakOptions>(
    builder.Configuration.GetSection(KeycloakOptions.SectionName));

var keycloakOptions = builder.Configuration
    .GetSection(KeycloakOptions.SectionName)
    .Get<KeycloakOptions>()
    ?? throw new InvalidOperationException("Keycloak configuration is missing");

// =============================================
// 2. DATABASE
// =============================================

builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

// =============================================
// 3. AUTHENTICATION & AUTHORIZATION
// =============================================

// Configure JWT Bearer authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = keycloakOptions.GetRealmUrl();
    options.Audience = keycloakOptions.Audience;
    options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = keycloakOptions.TokenValidation.ValidateIssuer,
        ValidIssuer = keycloakOptions.GetRealmUrl(),
        
        ValidateAudience = keycloakOptions.TokenValidation.ValidateAudience,
        ValidAudience = keycloakOptions.Audience,
        
        ValidateLifetime = keycloakOptions.TokenValidation.ValidateLifetime,
        ClockSkew = TimeSpan.FromSeconds(keycloakOptions.TokenValidation.ClockSkewSeconds),
        
        ValidateIssuerSigningKey = keycloakOptions.TokenValidation.ValidateIssuerSigningKey,
        RequireExpirationTime = keycloakOptions.TokenValidation.RequireExpirationTime,
        RequireSignedTokens = keycloakOptions.TokenValidation.RequireSignedTokens
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "Authentication failed");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            var userId = context.Principal?.FindFirst("sub")?.Value;
            logger.LogInformation("Token validated for user: {UserId}", userId);
            return Task.CompletedTask;
        }
    };
});

// Add authorization policies
builder.Services.AddWorkSpaceManagerAuthorization();

// =============================================
// 4. SERVICES
// =============================================

// HTTP Context Accessor (required for UserContextService)
builder.Services.AddHttpContextAccessor();

// Keycloak Service with HTTP Client
builder.Services.AddHttpClient<IKeycloakService, KeycloakService>(client =>
{
    client.BaseAddress = new Uri(keycloakOptions.Authority);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Identity Services
builder.Services.AddScoped<IKeycloakService, KeycloakService>();
builder.Services.AddScoped<IJitProvisioningService, JitProvisioningService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();

// =============================================
// 5. CORS
// =============================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebClient", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://app.workspacemanager.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// =============================================
// 6. API DOCUMENTATION
// =============================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WorkSpaceManager API",
        Version = "v1",
        Description = "Enterprise Workspace Booking System API"
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =============================================
// 7. CONTROLLERS
// =============================================

builder.Services.AddControllers();

// =============================================
// 8. LOGGING
// =============================================

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsProduction())
{
    // Add Application Insights in production
    builder.Services.AddApplicationInsightsTelemetry();
}

// =============================================
// BUILD APPLICATION
// =============================================

var app = builder.Build();

// =============================================
// 9. MIDDLEWARE PIPELINE
// =============================================

// Development tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "WorkSpaceManager API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger at root
    });
}

// HTTPS redirection (production only)
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// CORS
app.UseCors("AllowWebClient");

// Authentication & Authorization
app.UseAuthentication();
app.UseJwtAuthentication(); // Custom JWT middleware for additional validation
app.UseJitProvisioning(); // Automatic user provisioning
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

// =============================================
// 10. DATABASE MIGRATION (Development only)
// =============================================

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    
    try
    {
        await dbContext.Database.MigrateAsync();
        app.Logger.LogInformation("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database migration failed");
    }
}

// =============================================
// 11. RUN APPLICATION
// =============================================

app.Logger.LogInformation("Starting WorkSpaceManager Identity Service");
app.Logger.LogInformation("Keycloak Authority: {Authority}", keycloakOptions.Authority);
app.Logger.LogInformation("Keycloak Realm: {Realm}", keycloakOptions.Realm);

app.Run();
