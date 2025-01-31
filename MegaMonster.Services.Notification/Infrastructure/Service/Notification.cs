using MegaMonster.Services.Notification.Infrastructure.MailJet;
using MegaMonster.Services.Notification.Infrastructure.Reader;
using MegaMonster.Services.Notification.WebApi.Dto_s;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MegaMonster.Services.Notification.Infrastructure.Service;

public class Notification(IEmailSender emailSender, ITemplateReader templateReader) : INotification
{
    public async Task<bool> SendConfirmEmailAsync(UserDto userDto)
    {
        var templatePath = "Persistence/Templates/ConfirmRegister.html";
        var htmlBody = await templateReader.ReadTemplateAsync(templatePath);
        
        if (htmlBody == null)
        {
            return false;
        }
        
        htmlBody = htmlBody.Replace("{name}", string.Join(" ", userDto.FirstName, userDto.LastName))
            .Replace("{email}", userDto.Email)
            .Replace("{telegram}", userDto.Telegram);

        await emailSender.SendEmailAsync(userDto.Email, Wc.ConfirmEmail, htmlBody);
        return true;
    }
    public async Task<bool> SendBillEmailAsync(UserDto userDto, string url)
    {
        var templatePath = "Persistence/Templates/Bill.html";
        var htmlBody = await templateReader.ReadTemplateAsync(templatePath);
        
        if (htmlBody == null)
        {
            return false;
        }
        
        htmlBody = htmlBody.Replace("{name}", string.Join(" ", userDto.FirstName, userDto.LastName))
            .Replace("{email}", userDto.Email)
            .Replace("{telegram}", userDto.Telegram)
            .Replace("{url}", url);

        await emailSender.SendEmailAsync(userDto.Email, Wc.Information, htmlBody);
        return true;
    }
    public async Task<bool> SendBanEmailAsync(UserDto userDto, string reason)
    {
        var templatePath = "Persistence/Templates/BanUser.html";
        var htmlBody = await templateReader.ReadTemplateAsync(templatePath);
        
        if (htmlBody == null)
        {
            return false;
        }

        htmlBody = htmlBody.Replace("{name}", string.Join(" ", userDto.FirstName, userDto.LastName))
            .Replace("{email}", userDto.Email)
            .Replace("{reason}", reason); 

        await emailSender.SendEmailAsync(userDto.Email, Wc.BlockedUser, htmlBody);
        return true;
    }
    public async Task<bool> SendNewsEmailAsync(UserDto userDto)
    {
        var templatePath = "Persistence/Templates/News.html";
        var htmlBody = await templateReader.ReadTemplateAsync(templatePath);
        
        if (htmlBody == null)
        {
            return false;
        }
        
        await emailSender.SendEmailAsync(userDto.Email, Wc.News, htmlBody);
        return true;
    }
}