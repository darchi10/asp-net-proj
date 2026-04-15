using Microsoft.AspNetCore.Mvc;
using MobilePhoneServiceAndSalesSystem.Models.Repositories;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductMockRepository _productMockRepository;

        public ProductsController(IProductMockRepository productMockRepository)
        {
            _productMockRepository = productMockRepository;
        }

        public IActionResult Index()
        {
            return View(_productMockRepository.Items);
        }

        public IActionResult Details(int id)
        {
            var product = _productMockRepository.Items.FirstOrDefault(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}