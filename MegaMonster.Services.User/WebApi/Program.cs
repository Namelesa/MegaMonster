using System.Text;
using MassTransit;
using MegaMonster.Services.User.Application.Messaging.Consumer;
using MegaMonster.Services.User.Application.Services;
using MegaMonster.Services.User.Application.Validation.RoleValidator;
using MegaMonster.Services.User.Application.Validation.UserValidator;
using MegaMonster.Services.User.Core.Interfaces;
using MegaMonster.Services.User.Infrastructure.MessageBroker;
using MegaMonster.Services.User.Infrastructure.Redis;
using MegaMonster.Services.User.Persistence.Data;
using MegaMonster.Services.User.Persistence.DbInitializer;
using MegaMonster.Services.User.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "UserService";
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["JWTConfig:Issuer"],
            ValidAudience = builder.Configuration["JWTConfig:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTConfig:Key"] ?? string.Empty)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("access_token"))
                {
                    context.Token = context.Request.Cookies["access_token"];
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IRedisService, RedisService>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();

builder.Services.AddScoped<UserValidation>();
builder.Services.AddScoped<RoleValidation>();

builder.Services.AddScoped<IDbInitializer, DbInitializer>();

builder.Services.Configure<MessageBrokerSettings>(
    builder.Configuration.GetSection("MessageBroker"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);

builder.Services.AddMassTransit(busConfiguration =>
{
    busConfiguration.AddConsumer<UserConsumer>();
    
    busConfiguration.UsingRabbitMq((context, configurator) =>
    {
        MessageBrokerSettings settings = context.GetRequiredService<MessageBrokerSettings>();
        
        configurator.Host(new Uri(settings.Host), h =>
        {
            h.Username(settings.UserName);
            h.Password(settings.Password);
        });
        
        configurator.ReceiveEndpoint("user-service-queue", e =>
        {
            e.ConfigureConsumer<UserConsumer>(context);
        });
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await dbInitializer.Initialize();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
