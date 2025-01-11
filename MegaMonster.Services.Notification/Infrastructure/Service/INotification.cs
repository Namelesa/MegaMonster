using MegaMonster.Services.Notification.Dto_s;

namespace MegaMonster.Services.Notification.Infrastructure.Service;

public interface INotification
{
    Task<bool> SendConfirmEmailAsync(UserDto userDto);
    Task<bool> SendBillEmailAsync(UserDto userDto, string url);
    Task<bool> SendBanEmailAsync(UserDto userDto, string reason);
    Task<bool> SendNewsEmailAsync(UserDto userDto);
}