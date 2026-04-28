using MITCRMS.Models.DTOs;
using MITCRMS.Models.Enum;

namespace MITCRMS.Interface.Services
{
    public interface IReportReminderService
    {
        public Task<BaseResponse> ProcessRemindersAsync(ReminderLevel level);
    }
}
