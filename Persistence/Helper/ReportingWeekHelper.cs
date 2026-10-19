using System.Globalization;

namespace MITCRMS.Persistence.Helper
{
    public static class ReportingWeekHelper
    {
        private static readonly TimeZoneInfo ReportingTimeZone = GetReportingTimeZone();

        public static DateTime GetReportingLocalTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ReportingTimeZone);
        }

        public static (int Year, int WeekNumber) GetCurrentReportingWeek()
        {
            var now = GetReportingLocalTime();
            var week = ISOWeek.GetWeekOfYear(now);

            return (ISOWeek.GetYear(now), week);

        }

        private static TimeZoneInfo GetReportingTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Africa/Lagos");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.Utc;
            }
            catch (InvalidTimeZoneException)
            {
                return TimeZoneInfo.Utc;
            }
        }
    }
}
