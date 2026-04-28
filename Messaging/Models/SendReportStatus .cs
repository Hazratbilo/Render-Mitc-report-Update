namespace MITCRMS.Implementation.Messaging.Models
{
    public class SendReportStatus : Base
    {
#pragma warning disable CS8618
        public string Title { get; set; }
#pragma warning restore CS8618
#pragma warning disable CS8618
        public string Message { get; set; }
#pragma warning restore CS8618 
        public string ActionText { get; set; } = "Open Report Portal";

      
#pragma warning disable CS8618 
        public string Status { get; set; }          // Approved / Rejected
#pragma warning restore CS8618
#pragma warning disable CS8618 
        public string ActionLink { get; set; }      // Link to report
#pragma warning restore CS8618
        public DateTime SubmittedOn { get; set; }

    }
}