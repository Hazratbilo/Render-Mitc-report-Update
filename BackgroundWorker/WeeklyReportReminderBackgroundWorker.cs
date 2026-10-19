using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MITCRMS.Interface.Services;
using MITCRMS.Models.Enum;
using MITCRMS.Persistence.Helper;

namespace Mitc_report_Update.BackgroundWorker
{
    public class WeeklyReportReminderBackgroundWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WeeklyReportReminderBackgroundWorker> _logger;
        private DateOnly? _lastProcessedReminderDate;

        public WeeklyReportReminderBackgroundWorker(IServiceScopeFactory scopeFactory, ILogger<WeeklyReportReminderBackgroundWorker> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = ReportingWeekHelper.GetReportingLocalTime();
                    var reminderLevel = GetReminderLevel(now);

                    if (reminderLevel.HasValue)
                    {
                        var reminderDate = DateOnly.FromDateTime(now);
                        if (_lastProcessedReminderDate == reminderDate)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                            continue;
                        }

                        // Mark the slot before doing work so a slow send or a retry in this
                        // minute cannot send the same reminder twice in one process.
                        _lastProcessedReminderDate = reminderDate;
                        using var scope = _scopeFactory.CreateScope();
                        var service = scope.ServiceProvider
                            .GetRequiredService<IReportReminderService>();

                        var result = await service.ProcessRemindersAsync(reminderLevel.Value);
                        if (result.Status)
                        {
                            _logger.LogInformation("Weekly {ReminderLevel} reminders completed for {Date}.", reminderLevel, reminderDate);
                        }
                        else
                        {
                            _logger.LogWarning("Weekly {ReminderLevel} reminders did not complete for {Date}: {Message}", reminderLevel, reminderDate, result.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running weekly reminder job.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        internal static ReminderLevel? GetReminderLevel(DateTime now)
        {
            return now.DayOfWeek switch
            {
                DayOfWeek.Friday when now.Hour == 16 && now.Minute == 00
                    => ReminderLevel.Friendly,

                DayOfWeek.Saturday when now.Hour == 09 && now.Minute == 00
                    => ReminderLevel.FollowUp,

                DayOfWeek.Sunday when now.Hour == 17 && now.Minute == 00
                    => ReminderLevel.FinalNotice,

                _ => null
            };
        }
    }
}
