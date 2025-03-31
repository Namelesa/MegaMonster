using MegaMonster.Services.Notification.Core.Interfaces;
using MegaMonster.Services.Notification.Core.Models;
using MegaMonster.Services.Notification.Infrastructure.MailJet;
using MegaMonster.Services.Notification.Infrastructure.Reader;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MegaMonster.Services.Notification.Persistence;

public class Notification(IEmailSender emailSender, ITemplateReader templateReader) : INotification
{
    public async Task<bool> SendConfirmEmailAsync(UserDto userDto)
    {
        var templatePath = "Infrastructure/Templates/ConfirmRegister.html";
        var htmlBody = await templateReader.ReadTemplateAsync(templatePath);
        
        if (htmlBody == null)
        {
            return false;
        }

        htmlBody = htmlBody.Replace("{name}", string.Join(" ", userDto.UserName))
            .Replace("{email}", userDto.Email)
            .Replace("{link}", userDto.ConfirmLink);
        Console.WriteLine(userDto.ConfirmLink);
        await emailSender.SendEmailAsync(userDto.Email, Wc.ConfirmEmail, htmlBody);
        
        return true;
    }
    public async Task<bool> SendBillEmailAsync(BillUserDto userDto)
    {
        var templatePath = "Infrastructure/Templates/Bill.html";
        var htmlBody = await templateReader.ReadTemplateAsync(templatePath);
        
        if (htmlBody == null)
        {
            return false;
        }
        
        htmlBody = htmlBody.Replace("{UserName}", string.Join(" ", userDto.UserName))
            .Replace("{OrderId}", userDto.OrderId.ToString())
            .Replace("{PaymentType}", userDto.PaymentType)
            .Replace("{Email}", userDto.Email)
            .Replace("{OrderDetailsRows}", string.Join("<br>", userDto.OrderDetailsRows))  
            .Replace("{Sum}", userDto.Sum.ToString())
            .Replace("{Status}", userDto.Status);

        await emailSender.SendEmailAsync(userDto.Email, Wc.Information, htmlBody);
        return true;
    }
    public async Task<bool> SendBanEmailAsync(UserDto userDto, string reason)
    {
        var templatePath = "Infrastructure/Templates/BanUser.html";
        var htmlBody = await templateReader.ReadTemplateAsync(templatePath);
        
        if (htmlBody == null)
        {
            return false;
        }

        htmlBody = htmlBody.Replace("{name}", string.Join(" ", userDto.UserName))
            .Replace("{email}", userDto.Email)
            .Replace("{reason}", reason); 

        await emailSender.SendEmailAsync(userDto.Email, Wc.BlockedUser, htmlBody);
        return true;
    }
    public async Task<bool> SendNewsEmailAsync(UserDto userDto)
    {
        var templatePath = "Infrastructure/Templates/News.html";
        var htmlBody = await templateReader.ReadTemplateAsync(templatePath);
        
        if (htmlBody == null)
        {
            return false;
        }
        
        await emailSender.SendEmailAsync(userDto.Email, Wc.News, htmlBody);
        return true;
    }
}