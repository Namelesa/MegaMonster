using MegaMonster.Services.Favors.Application.Services;
using MegaMonster.Services.Favors.Core.Interfaces;
using MegaMonster.Services.Favors.Core.Models;
using MegaMonster.Services.Favors.Persistence.Data;
using MegaMonster.Services.Favors.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddScoped<TicketsService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<RideService>();
builder.Services.AddScoped<NewsService>();

builder.Services.AddTransient<IRideRepository, RideRepository>();
builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
builder.Services.AddTransient<ITicketRepository, TicketRepository>();
builder.Services.AddTransient<ITicketConfigurationRepository, TicketConfigurationRepository>();
builder.Services.AddTransient<INewsRepository, NewsRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.UseHttpsRedirection();

app.Run();
