using Microsoft.AspNetCore.Mvc;
using MobilePhoneServiceAndSalesSystem.Models.Repositories;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderMockRepository _orderMockRepository;

        public OrdersController(IOrderMockRepository orderMockRepository)
        {
            _orderMockRepository = orderMockRepository;
        }

        public IActionResult Index()
        {
            return View(_orderMockRepository.Items);
        }

        public IActionResult Details(int id)
        {
            var order = _orderMockRepository.Items.FirstOrDefault(o => o.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}