using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mitc_report_Update.Exceptions
{
    public class MailSenderException : Exception
    {
#pragma warning disable CS0114
        public string Message { get; set; }
#pragma warning restore CS0114

#pragma warning disable CS8618
        public MailSenderException(string message)
#pragma warning restore CS8618
            : base(message) { }

#pragma warning disable CS8618
        public MailSenderException(string message, Exception innerException)
#pragma warning restore CS8618
            : base(message, innerException) { }
    }
}