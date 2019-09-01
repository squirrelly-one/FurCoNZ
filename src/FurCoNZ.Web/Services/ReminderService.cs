using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Web.DAL;
using FurCoNZ.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FurCoNZ.Web.Services
{
    public class ReminderService : IReminderService
    {
        private readonly FurCoNZDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReminderService> _logger;
        private readonly IOrderService _orderService;
        private readonly IViewRenderService _viewRenderService;

        public ReminderService(
            FurCoNZDbContext dbContext,
            IEmailService emailService,
            ILogger<ReminderService> logger,
            IOrderService orderService,
            IViewRenderService viewRenderService
        )
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _logger = logger;
            _orderService = orderService;
            _viewRenderService = viewRenderService;
        }

        public async Task Send30DayRemainingPendingOrdersAsync(CancellationToken cancellationToken = default)
        {
            var ordersExpiringInOneMonthAndUnreminded = await _dbContext.Orders.Include(o => o.OrderedBy).Where(o => o.AmountOwingCents > 0).ToListAsync();

            foreach (var order in ordersExpiringInOneMonthAndUnreminded)
            {
                var message = await _viewRenderService.RenderToStringAsync("EmailTemplate/ThirtyDayReminder", new ThirtyDayReminderViewModel());
            }
        }
    }
}
