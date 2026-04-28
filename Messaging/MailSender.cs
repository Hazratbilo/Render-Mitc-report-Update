using brevo_csharp.Api;
using brevo_csharp.Model;
using Mitc_report_Update.Exceptions;
using MITCRMS.Extensions;
using MITCRMS.Interface.Messaging;

namespace MITCRMS.Implementation.Messaging
{
    public class MailSender(IConfiguration config, IWebHostEnvironment env,
        ILogger<MailSender> logger) : IMailSender
    {
        private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
        private readonly IWebHostEnvironment _env = env ?? throw new ArgumentNullException(nameof(env));
        private readonly ILogger<MailSender> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
#pragma warning disable CS8625 
        public async Task<bool> SendEmailAsync(string from, string fromName, string to, string toName, string subject, string body, IDictionary<string, Stream> attachments = null)
#pragma warning restore CS8625 
        {
            var smtpApiKey = _config["MITCRMSSettings:SmtpApiKey"];
            var apiInstance = new TransactionalEmailsApi();

           
            var sendSmtpEmail = new SendSmtpEmail
            {
                Sender = new SendSmtpEmailSender(fromName, from),
                To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(to, toName) },
                Subject = subject,
                HtmlContent = body
            };

            var emailAttachments = new List<SendSmtpEmailAttachment>();

                       foreach (var asset in EmailAssetRegistry.AssetMap)
            {
                if (body.Contains($"cid:{asset.Key}"))
                {
                    string root = _env.WebRootPath ?? _env.ContentRootPath;

                    string folderName = _env.WebRootPath == null ? Path.Combine("wwwroot", "EmailAssets") : "EmailAssets";

                    string filePath = Path.Combine(root, folderName, asset.Value.Replace('/', Path.DirectorySeparatorChar));

                    if (File.Exists(filePath))
                    {
                        byte[] imageBytes = File.ReadAllBytes(filePath);
                        emailAttachments.Add(new SendSmtpEmailAttachment(
                            name: asset.Key,
                            content: imageBytes
                        ));
                        _logger.LogInformation("Successfully attached asset: {AssetName}", asset.Key);
                    }
                    else
                    {
                        // This will now log the CORRECT path so you can debug it
                        _logger.LogWarning("Email asset not found at: {AssetPath}", filePath);
                    }
                }
            }

            // Process Additional Attachments
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    emailAttachments.Add(new SendSmtpEmailAttachment(
                        content: ReadFully(attachment.Value),
                        name: attachment.Key));
                }
            }
            if (emailAttachments.Count > 0)
            {
                sendSmtpEmail.Attachment = emailAttachments;
            }

            if (!string.IsNullOrEmpty(smtpApiKey))
            {
                brevo_csharp.Client.Configuration.Default.ApiKey["api-key"] = smtpApiKey;

                try
                {
                    await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError("Exception when calling TransactionalEmailsApi.SendTransacEmail: " + e.Message);
                    throw new MailSenderException(e.Message, e);
                }
            }

            _logger.LogError("SMTP API Key is not configured.");
            throw new MailSenderException("SMTP API Key is not configured.");
        }
        private static byte[] ReadFully(Stream input)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    input.CopyTo(ms);
                    return ms.ToArray();
                }
            }
    }
}
