using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TaskAide.API.Handlers;
using TaskAide.API.Services.Auth;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Repositories;
using TaskAide.Infrastructure.Data;
using TaskAide.Infrastructure.Repositories;
using TaskAide.API.Common;
using TaskAide.Domain.Services;
using TaskAide.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AddDatabase(builder);

builder.Services.AddScoped<IEncryptionService, EncryptionService>(provider =>
{
    var key = builder.Configuration[Constants.Configuration.Encryption.Key];
    return new EncryptionService(Encoding.UTF8.GetBytes(key));
});

AddAuthentication(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<TaskAideContext>();
db.Database.Migrate();
var dbSeeder = app.Services.CreateScope().ServiceProvider.GetRequiredService<AuthDbSeeder>();
await dbSeeder.SeedAsync();

app.UseMiddleware<ExceptionHandler>();

app.Run();

static void AddAuthentication(WebApplicationBuilder builder)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
        //.AddGoogle(googleOptions =>
        //{
        //    googleOptions.ClientId = builder.Configuration[Constants.Configuration.Authentication.Google.ClientId];
        //    googleOptions.ClientSecret = builder.Configuration[Constants.Configuration.Authentication.Google.ClientSecret];
        //})
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters.ValidateIssuer = true;
            options.TokenValidationParameters.ValidateAudience = true;
            options.TokenValidationParameters.ValidateLifetime = true;
            options.TokenValidationParameters.ValidateIssuerSigningKey = true;
            options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
            options.TokenValidationParameters.ValidAudience = builder.Configuration[Constants.Configuration.Jwt.ValidAudience];
            options.TokenValidationParameters.ValidIssuer = builder.Configuration[Constants.Configuration.Jwt.ValidIssuer];
            options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration[Constants.Configuration.Jwt.Secret]));
        });
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<AuthDbSeeder>();
}

static void AddDatabase(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<TaskAideContext>(options =>
    {
        var connectionString = "Data Source=localhost;Initial Catalog=TaskAide;User ID=sa;Password=TaskA1deComplexP@ssw0rd!;encrypt=false";
        options.UseSqlServer(connectionString);
    });

    builder.Services.AddIdentity<User, IdentityRole>()
        .AddEntityFrameworkStores<TaskAideContext>()
        .AddDefaultTokenProviders();
    builder.Services.AddScoped<IRefrehTokenRepository, RefreshTokenRepository>();
}