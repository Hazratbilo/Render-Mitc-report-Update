namespace MITCRMS.Implementation.Messaging.Models
{
    public class SendReportReminder : Base
    {
#pragma warning disable CS8618
        public string Title { get; set; }
#pragma warning restore CS8618
        #pragma warning disable CS8618
        public string Message { get; set; }
#pragma warning restore CS8618
        public string ActionText { get; set; } = "Login to Portal";
        public string ActionLink { get; set; }
    }
}
