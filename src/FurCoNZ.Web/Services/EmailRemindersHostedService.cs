using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FurCoNZ.Web.Services
{
    internal class EmailRemindersHostedService : IHostedService, IDisposable
    {
        private readonly IReminderService _reminderService;
        private readonly ILogger _logger;

        // Needs to be defined as a field otherwise it will get sweeped up by the GC
        private Timer _30DayUnfulfilledOrderTimer;

        public EmailRemindersHostedService(IReminderService reminderService, ILogger<EmailRemindersHostedService> logger)
        {
            _reminderService = reminderService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");

            const int triggerHour24Hour = 9; // 9am

            var todayTriggerTime = DateTime.Now.Date.AddHours(triggerHour24Hour); 
            var nextTriggerTime = DateTime.Now <= todayTriggerTime ? todayTriggerTime : todayTriggerTime.AddDays(1);
            var timeUntilNextTrigger = nextTriggerTime - DateTime.Now;

            _30DayUnfulfilledOrderTimer = new Timer(Send30DayRemainingPendingOrders, null, timeUntilNextTrigger, TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private void Send30DayRemainingPendingOrders(object state)
        {
            _logger.LogInformation($"Running Task: {nameof(Send30DayRemainingPendingOrders)}.");

            Task.Run(() => _reminderService.Send30DayRemainingPendingOrdersAsync()).GetAwaiter().GetResult();

            _logger.LogInformation($"Completed Task: {nameof(Send30DayRemainingPendingOrders)}.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");

            _30DayUnfulfilledOrderTimer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _30DayUnfulfilledOrderTimer?.Dispose();
        }
    }
}
