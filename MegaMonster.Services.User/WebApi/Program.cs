using System.Text;
using System.Text.Json;
using FluentValidation;
using MassTransit;
using MegaMonster.Services.User.Application.Messaging;
using MegaMonster.Services.User.Application.Role;
using MegaMonster.Services.User.Application.User;
using MegaMonster.Services.User.Core.Interfaces;
using MegaMonster.Services.User.Core.Role;
using MegaMonster.Services.User.Core.User;
using MegaMonster.Services.User.Infrastructure.MessageBroker;
using MegaMonster.Services.User.Infrastructure.Redis;
using MegaMonster.Services.User.Persistence.Data;
using MegaMonster.Services.User.Persistence.Data.DbInitializer;
using MegaMonster.Services.User.Persistence.Role;
using MegaMonster.Services.User.Persistence.User;
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

builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.IncludeFields = true;
    options.PropertyNameCaseInsensitive = true;
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
builder.Services.AddScoped<IBannedUserRepository, BannedUserRepository>();

builder.Services.AddScoped<IRedisService, RedisService>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();

builder.Services.AddScoped<IValidator<Users>, UserValidation>();
builder.Services.AddScoped<IValidator<Role>, RoleValidation>();

builder.Services.AddScoped<IDbInitializer, DbInitializer>();

builder.Services.Configure<MessageBrokerSettings>(
    builder.Configuration.GetSection("MessageBroker"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);

builder.Services.AddMassTransit(busConfiguration =>
{
    busConfiguration.AddConsumer<UserConsumer>();
    busConfiguration.AddConsumer<UserInfoConsumer>();
    busConfiguration.AddConsumer<UserEmailConsumer>();
    busConfiguration.AddConsumer<UserConfirmConsumer>();
    busConfiguration.AddConsumer<UserBanRollBackConsumer>();
    busConfiguration.AddConsumer<UserEditInfoRollBackConsumer>();
    
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
        configurator.ReceiveEndpoint("user-get-login-queue", e =>
        {
            e.ConfigureConsumer<UserInfoConsumer>(context);
        });
        configurator.ReceiveEndpoint("user-get-email-queue", e =>
        {
            e.ConfigureConsumer<UserEmailConsumer>(context);
        });
        configurator.ReceiveEndpoint("user-confirm-email-queue", e =>
        {
            e.ConfigureConsumer<UserConfirmConsumer>(context);
        });
        configurator.ReceiveEndpoint("user-ban-rollback-queue", e =>
        {
            e.ConfigureConsumer<UserBanRollBackConsumer>(context);
        });
        configurator.ReceiveEndpoint("user-edit-rollback-queue", e =>
        {
            e.ConfigureConsumer<UserEditInfoRollBackConsumer>(context);
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
