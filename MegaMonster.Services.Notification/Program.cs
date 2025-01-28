using MegaMonster.Services.Notification.Infrastructure.MailJet;
using MegaMonster.Services.Notification.Infrastructure.Reader;
using MegaMonster.Services.Notification.Infrastructure.Service;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<INotification, Notification>();
builder.Services.AddTransient<ITemplateReader, TemplateReader>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
