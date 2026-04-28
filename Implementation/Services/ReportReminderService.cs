using MailKit;
using MITCRMS.Interface.Repository;
using MITCRMS.Interface.Services;
using MITCRMS.Models.DTOs;
using MITCRMS.Models.Enum;

namespace MITCRMS.Implementation.Services
{
    public class ReportReminderService(IReportRepository reportRepository,
    Mitc_report_Update.Interface.MailingService.IMailService mailService) : IReportReminderService
    {
        private readonly IReportRepository _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
        private readonly Mitc_report_Update.Interface.MailingService.IMailService _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        public async Task<BaseResponse> ProcessRemindersAsync(ReminderLevel level)
        {
            var defaulters = await _reportRepository.GetStaffWithoutReportWeek();

            if (defaulters is null || !defaulters.Any())
            {
                return new BaseResponse
                {
                    Message = "No report defaulters",
                    Status = false
                };
            }
            bool anyFailure = false;

            foreach (var staff in defaulters)
            {
                var (subject, body) = BuildMessage(level);
                Console.WriteLine($"Email: {staff.Email}");
                var isSent = await _mailService.SendReminderMail(staff.Email, staff.FirstName, subject, body);

                if (!isSent) anyFailure = true;

            }
            return new BaseResponse
            {
                Message = anyFailure ? "Some reminders failed to send" : "All reminders sent successfully",
                Status = !anyFailure
            };
        }

        private (string Subject, string Body) BuildMessage(ReminderLevel level)
        {
            return level switch
            {
                ReminderLevel.Friendly =>
                ("Weekly Report Reminder",
                "Kindly submit your weekly report before close of Business"),

                ReminderLevel.FollowUp =>
                ("Weekly Report Follow-Up",
                "This is a follow-up reminder to submit your weekly report. Please ensure it is submitted before the end of the day."),

                ReminderLevel.FinalNotice =>
                ("Final Notice - Weekly Report",
                "Final reminder: Please submit your weekly report immediately"),



                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
