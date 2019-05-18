using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using Stripe.Checkout;

using FurCoNZ.Configuration;
using FurCoNZ.DAL;

namespace FurCoNZ.Services.Payment.Stripe
{
    public class StripeHostedService : IHostedService, IDisposable
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IServiceProvider _services;
        private readonly IOptions<StripeSettings> _options;
        private readonly ILogger _logger;
        private readonly EventService _eventService;

        private Timer _timer;
        private EventListOptions _eventListOptions;

        private CancellationTokenSource _cancellationTokenSource;

        public static TimeSpan PollInterval = TimeSpan.FromMinutes(5);

        public StripeHostedService(IHostingEnvironment hostingEnvironment, IServiceProvider serviceProvider, IOptions<StripeSettings> options, ILogger<StripeHostedService> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _services = serviceProvider;
            _options = options;
            _logger = logger;

            _eventService = new EventService();

            if (_hostingEnvironment.IsDevelopment())
            {
                PollInterval = TimeSpan.FromSeconds(30);
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting event checking service");
            _cancellationTokenSource = new CancellationTokenSource();

            _timer?.Dispose();
            _timer = new Timer(CheckEvents, null, TimeSpan.FromSeconds(30), PollInterval);

            // Initially get events from the past 24 hours.
            _eventListOptions = new EventListOptions
            {
                CreatedRange = new DateRangeOptions {
                    GreaterThan = DateTime.Now.Subtract(TimeSpan.FromDays(1)),
                },
            };

            return Task.CompletedTask;
        }

        private async void CheckEvents(object state)
        {
            _logger.LogInformation($"Polling for events of type {_eventListOptions.Type}");

            var cancellationToken = _cancellationTokenSource?.Token 
                ?? throw new Exception("CancellationTokenSource must not be null");

            using (var serviceScope = _services.CreateScope())
            {
                var stripeService = serviceScope.ServiceProvider.GetRequiredService<StripeService>();
                StripeConfiguration.SetApiKey(_options.Value.SecretKey);

                var lastEventDateTime = _eventListOptions.CreatedRange.GreaterThan;

                // Clear the pagination cursor.
                _eventListOptions.StartingAfter = null;

                StripeList<Event> events;
                do
                {
                    // List events, filtered by _eventListOptions
                    events = await _eventService.ListAsync(_eventListOptions, cancellationToken: cancellationToken);

                    foreach (var stripeEvent in events)
                    {
                        _logger.LogInformation($"Received event {stripeEvent.Type} ({stripeEvent.Id})");

                        switch(stripeEvent.Data.Object)
                        { 
                            case Session sessionEvent:
                                await stripeService.FufillOrderFromCompletedSessionAsync(sessionEvent, cancellationToken);
                                break;
                            case Charge chargeEvent:
                                await stripeService.ProcessChargeAsync(chargeEvent, cancellationToken);
                                break;
                            default:
                                break;
                        }
                        

                        // When there are more events than the limit allowed returned by Stripe API, 
                        //  use pagination by specifying the last event received.
                        _eventListOptions.StartingAfter = stripeEvent.Id;

                        if(lastEventDateTime < stripeEvent.Created.Value)
                            lastEventDateTime = stripeEvent.Created.Value;
                    }
                } while (events.Any());

                // Update the event window to prevent the same events from being processed again.
                _eventListOptions.CreatedRange.GreaterThan = lastEventDateTime;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stopping event checking service");
            _cancellationTokenSource?.Cancel();
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}
