using Backend.models;
using Microsoft.EntityFrameworkCore;
using Backend.Service;

var builder = WebApplication.CreateBuilder(args);

//services

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("connection string is null")));


builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

//middleswares

app.MapControllers();  

app.Run();
  