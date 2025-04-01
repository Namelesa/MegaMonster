using MegaMonster.Services.Notification.Core.User;

namespace MegaMonster.Services.Notification.Core.Interfaces;

public interface INotification
{
    Task<bool> SendConfirmEmailAsync(UserDto userDto);
    Task<bool> SendBillEmailAsync(BillUserDto userDto);
    Task<bool> SendBanEmailAsync(UserDto userDto, string reason);
    Task<bool> SendNewsEmailAsync(UserDto userDto);
}