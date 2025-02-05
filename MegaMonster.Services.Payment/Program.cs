using MegaMonster.Services.Payment.Infrastructure.Service;
using MegaMonster.Services.Payment.Persistence.Data;
using MegaMonster.Services.Payment.Persistence.DbInitializer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "PaymentService";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
var publicKey = builder.Configuration["LiqPay:publicKey"] ?? throw new ArgumentNullException("LiqPay:publicKey not configured");
var privateKey = builder.Configuration["LiqPay:secretKey"] ?? throw new ArgumentNullException("LiqPay:secretKey not configured");
builder.Services.AddScoped(options => new PaymentService(publicKey, privateKey, options.GetRequiredService<AppDbContext>()));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<IDbInitializer, DbInitializer>();

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
app.MapControllers();

app.UseCors("AllowAll");

app.Run();