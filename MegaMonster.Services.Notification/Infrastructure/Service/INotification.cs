using MegaMonster.Services.Notification.Dto_s;

namespace MegaMonster.Services.Notification.Infrastructure.Service;

public interface INotification
{
    Task<bool> SendConfirmEmailAsync(UserDto userDto);
}