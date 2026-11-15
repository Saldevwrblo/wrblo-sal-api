namespace Wrblo.SAL.Core.Implementation.Repositories;

public interface INotificationService
{
    Task<string> SendSmsAsync(string phoneNumber, string message);
}
