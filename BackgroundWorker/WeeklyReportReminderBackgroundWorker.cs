using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MITCRMS.Interface.Services;
using MITCRMS.Models.Enum;

namespace Mitc_report_Update.BackgroundWorker
{
    public class WeeklyReportReminderBackgroundWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WeeklyReportReminderBackgroundWorker> _logger;

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
                    var reminderLevel = GetReminderLevel();

                    if (reminderLevel.HasValue)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var service = scope.ServiceProvider
                            .GetRequiredService<IReportReminderService>();

                        await service.ProcessRemindersAsync(reminderLevel.Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running weekly reminder job.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private ReminderLevel? GetReminderLevel()
        {
            var now = DateTime.Now;

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