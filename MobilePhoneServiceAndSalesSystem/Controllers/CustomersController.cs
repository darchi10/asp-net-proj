using Microsoft.AspNetCore.Mvc;
using MobilePhoneServiceAndSalesSystem.Models.Repositories;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ICustomerMockRepository _customerMockRepository;

        public CustomersController(ICustomerMockRepository customerMockRepository)
        {
            _customerMockRepository = customerMockRepository;
        }

        public IActionResult Index()
        {
            return View(_customerMockRepository.Items);
        }

        public IActionResult Details(int id)
        {
            var customer = _customerMockRepository.Items.FirstOrDefault(c => c.Id == id);

            if (customer is null)
            {
                return NotFound();
            }

            return View(customer);
        }
    }
}