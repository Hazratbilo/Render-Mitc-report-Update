using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mitc_report_Update.Extensions.Exceptions
{
    public class RazorEngineException : Exception
    {
#pragma warning disable CS0114
        public string Message { get; set; }
#pragma warning disable CS8618 
        public RazorEngineException(string message)
#pragma warning restore CS8618
    : base(message) { }


#pragma warning disable CS8618
        public RazorEngineException(string message, Exception innerException)
#pragma warning restore CS8618
            : base(message, innerException) { }


    }
}