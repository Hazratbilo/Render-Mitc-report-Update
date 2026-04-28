using System.Globalization;

namespace MITCRMS.Persistence.Helper
{
    public static class ReportingWeekHelper
    {
        public static (int Year, int WeekNumber) GetCurrentReportingWeek()
        {
            var now = DateTime.Now;
            var week = ISOWeek.GetWeekOfYear(now);

            return (now.Year, week);

        }
    }
}
