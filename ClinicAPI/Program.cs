using System.Text;
using ClinicAPI.Data;
using ClinicAPI.Hubs;
using ClinicAPI.Models;
using ClinicAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// ── ASP.NET Identity ───────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ClinicDbContext>()
.AddDefaultTokenProviders();

// ── JWT Authentication ─────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    // Allow SignalR to receive the JWT from query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

// ── CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClinicPolicy", policy =>
        policy.WithOrigins(
                "https://localhost:7001", "http://localhost:5001",  // ClinicMVC
                "https://localhost:7002", "http://localhost:5002"   // ClinicReport
              )

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[]
    {
        "https://localhost:7001", "http://localhost:5001",
        "https://localhost:7002", "http://localhost:5002",
        "https://localhost:7269", "https://localhost:7283", "http://localhost:5283"
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClinicPolicy", policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ── SignalR ────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── Application Services ───────────────────────────────────────────────────
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// ── Controllers + Swagger ─────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Clinic API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Seed database ──────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitialiseAsync(scope.ServiceProvider);
}

// ── Middleware Pipeline ────────────────────────────────────────────────────


    app.UseSwagger();
    app.UseSwaggerUI();

    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await SeedData.InitialiseAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database seeding failed — app will still start");
    }
}

// ── Middleware Pipeline ────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("ClinicPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();
app.MapHub<AppointmentHub>("/hubs/appointments");

app.Run();
