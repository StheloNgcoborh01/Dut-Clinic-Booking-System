using Backend.models;
using Microsoft.EntityFrameworkCore;
using Backend.Service;
using Backend.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using dotenv.net;
using AspNetCoreRateLimit;
using System.Net;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

//services

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("connection string is null")));


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"];
var key = Encoding.ASCII.GetBytes(secret ?? throw new ArgumentNullException("connection string is null"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Rate Limiting Configuration
builder.Services.AddMemoryCache();

builder.Services.Configure<IpRateLimitOptions>(options =>
{
   options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/api/Auth/Register",
            Period = "1h",
            Limit = 3
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/Auth/Verify",
            Period = "1h",
            Limit = 3
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/Auth/Forgot",
            Period = "1h",
            Limit = 2
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/Auth/verifyForgot",
            Period = "1h",
            Limit = 5
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/Auth/login",
            Period = "1m",
            Limit = 3
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/Auth/Forgot",
            Period = "30m",
            Limit = 3
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/Bookings",
            Period = "1m",
            Limit = 10
        },
        new RateLimitRule
        {
            Endpoint = "GET:/api/Bookings/available-dates",
            Period = "1m",
            Limit = 20
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/Contact/SendMessage",
            Period = "1h",
            Limit = 3
        },
       
        new RateLimitRule
        {
            Endpoint = "GET:/api/Admin/TodaysBookings",
            Period = "1m",
            Limit = 30
        },
        new RateLimitRule
        {
            Endpoint = "GET:/api/Admin/verifyAdmin",
            Period = "1m",
            Limit = 30
        },
        new RateLimitRule
        {
            Endpoint = "GET:/api/Admin/totalBookingsCount",
            Period = "1m",
            Limit = 30
        },
        new RateLimitRule
        {
            Endpoint = "GET:/api/Admin/unreadMessagesCount",
            Period = "1m",
            Limit = 30
        },
        new RateLimitRule
        {
            Endpoint = "GET:/api/Admin/allUsers",
            Period = "1m",
            Limit = 30
        },
        // Fallback for everything else
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 50
        }
    };
});

builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// Add this before builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", 
             "https://your-frontend.azurewebsites.net")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseCors("AllowReactApp");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

//middleswares
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }