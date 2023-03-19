using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using TaskAide.API.Common;
using TaskAide.API.DTOs;
using TaskAide.API.Handlers;
using TaskAide.API.Handlers.Auth;
using TaskAide.API.Services.Auth;
using TaskAide.API.Services.Categories;
using TaskAide.Domain.Entities.Auth;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Repositories;
using TaskAide.Domain.Services;
using TaskAide.Infrastructure.Data;
using TaskAide.Infrastructure.Repositories;
using TaskAide.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AddDatabase(builder);

builder.Services.AddScoped<IEncryptionService, EncryptionService>(provider =>
{
    var key = builder.Configuration[Constants.Configuration.Encryption.Key];
    return new EncryptionService(Encoding.UTF8.GetBytes(key));
});
builder.Services.AddScoped<IProvidersService, ProvidersService>();
builder.Services.AddScoped<IBookingService, BookingService>();
AddMapper(builder);

AddAuthentication(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(builder =>
{
    builder.WithOrigins("https://localhost:8100")
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials();
});

app.UseCookiePolicy();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await SeedDatabaseAsync(app);

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

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(PolicyNames.BookingOwner, policy => policy.Requirements.Add(new BookingOwnerRequirement()));
    });

    builder.Services.AddScoped<IAuthorizationHandler, BookingOwnerAuthorizationHandler>();
}

static void AddDatabase(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<TaskAideContext>(options =>
    {
        var connectionString = "Data Source=localhost;Initial Catalog=TaskAide;User ID=sa;Password=TaskA1deComplexP@ssw0rd!;encrypt=false";
        options.UseSqlServer(connectionString, options =>
        {
            options.UseNetTopologySuite();
        });
    });

    builder.Services.AddIdentity<User, IdentityRole>()
        .AddEntityFrameworkStores<TaskAideContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddScoped<IRefrehTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
    builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
    builder.Services.AddScoped<IProviderServiceRepository, ProviderServiceRepository>();
    builder.Services.AddScoped<IBookingRepository, BookingRepository>();

    builder.Services.AddScoped<AuthDbSeeder>();
    builder.Services.AddScoped<CategoryDbSeeder>();
}

static async Task SeedDatabaseAsync(WebApplication app)
{
    var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<TaskAideContext>();
    db.Database.Migrate();

    var authDbSeeder = app.Services.CreateScope().ServiceProvider.GetRequiredService<AuthDbSeeder>();
    await authDbSeeder.SeedAsync();

    var categoryDbSeeder = app.Services.CreateScope().ServiceProvider.GetRequiredService<CategoryDbSeeder>();
    await categoryDbSeeder.SeedAsync();
}

static void AddMapper(WebApplicationBuilder builder)
{
    var mapperConfig = new MapperConfiguration(mc =>
    {
        mc.AddProfile(new MappingProfile());
    });
    var mapper = mapperConfig.CreateMapper();
    builder.Services.AddSingleton<IMapper>(mapper);
}