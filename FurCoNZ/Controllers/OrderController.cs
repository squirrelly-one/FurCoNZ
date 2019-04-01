using System.Threading;
using System.Threading.Tasks;
using FurCoNZ.Services;
using FurCoNZ.ViewModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FurCoNZ.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var viewModel = new TicketIndexViewModel
            {
                AvailableTicketTypes = await _orderService.GetTicketTypesAsync(cancellationToken: cancellationToken)
            };

            return View(viewModel);
        }
    }
}
