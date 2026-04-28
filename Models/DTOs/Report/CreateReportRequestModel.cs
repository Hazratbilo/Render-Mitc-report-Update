namespace MITCRMS.Models.DTOs.Report
{
    public class CreateReportRequestModel
    {
     
            public Guid DepartmentId { get; set; }
#pragma warning disable CS8618 
        public string Tittle { get; set; }
#pragma warning restore CS8618
#pragma warning disable CS8618 
        public string Content { get; set; }
#pragma warning restore CS8618
        public string? FileUrl { get; set; } = null;

    }
}
