using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FurCoNZ.Web.Services
{
    internal class EmailRemindersHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        // Needs to be defined as a field otherwise it will get sweeped up by the GC
        private Timer _30DayUnfulfilledOrderTimer;
        private Timer _CancelOrderNotificationTimer;

        public EmailRemindersHostedService(IServiceProvider serviceProvider, ILogger<EmailRemindersHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Email Reminders hosted service is starting.");

            const int triggerHour24Hour = 9; // 9am

            var todayTriggerTime = DateTime.Now.Date.AddHours(triggerHour24Hour); 
            var nextTriggerTime = DateTime.Now <= todayTriggerTime ? todayTriggerTime : todayTriggerTime.AddDays(1);
            var timeUntilNextTrigger = nextTriggerTime - DateTime.Now;

            _30DayUnfulfilledOrderTimer = new Timer(Send30DayRemainingPendingOrders, null, timeUntilNextTrigger, TimeSpan.FromDays(1));
            _CancelOrderNotificationTimer = new Timer(SendCancelOrderNotifications, null, timeUntilNextTrigger, TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private void Send30DayRemainingPendingOrders(object state) // TODO: Arbitrary day counts
        {
            _logger.LogInformation($"Running Task: {nameof(Send30DayRemainingPendingOrders)}.");

            using (var serviceScope = _serviceProvider.CreateScope())
            {
                var reminderService = serviceScope.ServiceProvider.GetRequiredService<IReminderService>();
                Task.Run(() => reminderService.NotifyOfPendingOrderAsync()).GetAwaiter().GetResult();
            }

            _logger.LogInformation($"Completed Task: {nameof(Send30DayRemainingPendingOrders)}.");
        }

        private void SendCancelOrderNotifications(object state)
        {
            _logger.LogInformation($"Running Task: {nameof(SendCancelOrderNotifications)}.");

            using (var serviceScope = _serviceProvider.CreateScope())
            {
                var reminderService = serviceScope.ServiceProvider.GetRequiredService<IReminderService>();
                Task.Run(() => reminderService.NotifyOfCancelledOrderAsync()).GetAwaiter().GetResult();
            }

            _logger.LogInformation($"Completed Task: {nameof(SendCancelOrderNotifications)}.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Email Reminders hosted service is stopping.");

            _30DayUnfulfilledOrderTimer?.Change(Timeout.Infinite, 0);
            _CancelOrderNotificationTimer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _30DayUnfulfilledOrderTimer?.Dispose();
            _CancelOrderNotificationTimer?.Dispose();
        }
    }
}
