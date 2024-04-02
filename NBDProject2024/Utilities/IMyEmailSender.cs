using NBDProject2024.ViewModels;

namespace NBDProject2024.Utilities
{
    public interface IMyEmailSender
    {
        Task SendOneAsync(string name, string email, string subject,
            string htmlMessage);
        Task SendToManyAsync(EmailMessage emailMessage);
    }
}
