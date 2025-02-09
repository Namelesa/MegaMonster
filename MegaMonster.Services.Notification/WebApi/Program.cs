using MassTransit;
using MegaMonster.Services.Notification.Application.Messaging;
using MegaMonster.Services.Notification.Application.Validator;
using MegaMonster.Services.Notification.Core.Interfaces;
using MegaMonster.Services.Notification.Infrastructure.MailJet;
using MegaMonster.Services.Notification.Infrastructure.MessageBroker;
using MegaMonster.Services.Notification.Infrastructure.Reader;
using MegaMonster.Services.Notification.Persistence;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<INotification, Notification>();
builder.Services.AddTransient<ITemplateReader, TemplateReader>();
builder.Services.AddScoped<UserValidator>();

builder.Services.Configure<MessageBrokerSettings>(
    builder.Configuration.GetSection("MessageBroker"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MessageBrokerSettings>>().Value);

builder.Services.AddMassTransit(busConfiguration =>
{
    busConfiguration.AddConsumer<NotifyUserConsumer>();
    busConfiguration.AddConsumer<UserBanConsumer>();
    
    busConfiguration.UsingRabbitMq((context, configurator) =>
    {
        MessageBrokerSettings settings = context.GetRequiredService<MessageBrokerSettings>();
        
        configurator.Host(new Uri(settings.Host), h =>
        {
            h.Username(settings.UserName);
            h.Password(settings.Password);
        });
        
        configurator.ReceiveEndpoint("notification-service-queue", e =>
        {
            e.ConfigureConsumer<NotifyUserConsumer>(context);
        });
        configurator.ReceiveEndpoint("ban-user-queue", e =>
        {
            e.ConfigureConsumer<UserBanConsumer>(context);
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

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
