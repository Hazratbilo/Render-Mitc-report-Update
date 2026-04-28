using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mitc_report_Update.Interface.MailingService
{
    public interface IMailService
    {
        public Task<bool> SendReminderMail(string email, string name, string title, string body);
    public Task<bool> SendReportStatus(string email, string name, string title, string status);
}
    
}