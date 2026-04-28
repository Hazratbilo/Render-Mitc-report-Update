using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using brevo_csharp.Model;
using Microsoft.Extensions.Options;
using Mitc_report_Update.Configuration;
using Mitc_report_Update.Exceptions;
using Mitc_report_Update.Extensions.Exceptions;
using Mitc_report_Update.Interface.MailingService;
using Mitc_report_Update.Interface.TemplateEngine;
using MITCRMS.Implementation.Messaging.Models;
using MITCRMS.Interface.Messaging;
using MITCRMS.Models.Entities;
using MITCRMS.Models.Enum;

namespace Mitc_report_Update.Implementation.MailingService
{
    public class MailService(IMailSender mailSender, IRazorEngine razorEngine,
        IOptions<EmailConfiguration> options, ILogger<MailService> logger) : IMailService
    {
        private readonly IMailSender _mailSender = mailSender ?? throw new ArgumentNullException(nameof(mailSender));
        private readonly IRazorEngine _razorEngine = razorEngine ?? throw new ArgumentNullException(nameof(razorEngine));
        private readonly EmailConfiguration _emailConfiguration = options.Value ?? throw new ArgumentException(nameof(options));
        private readonly ILogger<MailService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<bool> SendReminderMail(string email, string name, string title, string body)
        {
            try
            {
                var model = new SendReportReminder()
                {
                    Name = name,
                    Email = email,
                    Title = title,
                    Message = body

                };
                var mailBody = await _razorEngine.ParseAsync("SendReportReminderMail", model);
                return await _mailSender.SendEmailAsync(_emailConfiguration.FromEmail, _emailConfiguration.FromName, email, name, title, mailBody);
            }
            catch (RazorEngineException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            catch (MailSenderException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> SendReportStatus(string email, string name, string title, string status)
        {
            try
            {
                var model = new SendReportStatus()
                {
                    Name = name,
                    Email = email,
                    Title = title,
                    Message = status, 
                    Status = status,
                    SubmittedOn = DateTime.UtcNow,

                };

                var mailBody = await _razorEngine.ParseAsync("SendReportStatusMail", model);


#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                return await _mailSender.SendEmailAsync(
                    _emailConfiguration.FromEmail,
                    _emailConfiguration.FromName,
                    email,
                    name,
                    title,
                    mailBody,
                    null
                );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
            catch (RazorEngineException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            catch (MailSenderException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        //        public async Task<bool> SendReportStatus(string email, string name, string title, string status)
        //        {
        //            try
        //            {
        //                var subject = $"Report {status}";
        //                var headerColor = status switch
        //                {
        //                    "Approved" => "#1B2A4A",
        //                    "Rejected" => "#dc3545",
        //                    "Pending" => "#ffc107",
        //                    _ => "#6c757d"

        //                };

        //                var body = $@"
        //<table role=""presentation"" width=""100%"" style=""background:#f0f4f8; padding:40px 15px; font-family: Arial, sans-serif;"">
        //    <tr>
        //        <td align=""center"">
        //            <table role=""presentation"" width=""600"" style=""background:#ffffff; border-radius:12px; box-shadow: 0 4px 12px rgba(0,0,0,0.08); overflow:hidden;"">
        //                <!-- Header -->
        //                <tr>
        //                    <td style=""padding:35px; background:{headerColor}; text-align:center;"">
        //                        <img src=""cid:mitc_logo.png"" alt=""MITC Logo"" style=""height:60px; margin-bottom:10px;"" />
        //                        <div style=""font-size:24px; font-weight:700; color:#ffffff; letter-spacing:-0.5px;"">Report Notification</div>
        //                    </td>
        //                </tr>

        //                <!-- Body -->
        //                <tr>
        //                    <td style=""padding:40px 30px; color:#1e293b;"">
        //                        <p style=""font-size:16px; margin:0;"">Hello <strong>{name}</strong>,</p>
        //                        <p style=""font-size:16px; color:#475569; line-height:1.7; margin-top:10px;"">
        //                            Your report titled <b>{title}</b> has been <b>{status}</b>.
        //                        </p>

        //                        <!-- Button -->
        //                        <div style=""margin:30px 0; text-align:center;"">
        //                            <a href=""https://buildhedge.com""
        //                               style=""background:#1d4ed8; color:#ffffff; padding:14px 28px; border-radius:8px; font-weight:600; text-decoration:none; display:inline-block; font-size:16px;"">
        //                                Open Report Portal
        //                            </a>
        //                        </div>

        //                        <!-- Footer note -->
        //                        <p style=""font-size:12px; color:#94a3b8; text-align:center; margin-top:40px;"">
        //                            This is an automated reminder from the <strong>MITCRMS Lifecycle Engine</strong>.
        //                        </p>
        //                    </td>
        //                </tr>

        //            </table>
        //        </td>
        //    </tr>
        //</table>";

        //                return await _mailSender.SendEmailAsync(_emailConfiguration.FromEmail, _emailConfiguration.FromName, email, name, subject, body);
        //            }
        //            catch
        //            {
        //                return false;
        //            }
        //        }
    }
}