namespace MITCRMS.Interface.Messaging
{
    public interface IMailSender
    {
        Task<bool> SendEmailAsync(string from, string fromName,  string to, string toName, string subject, string body, IDictionary<string, Stream> attachments = null!);
    }
}
