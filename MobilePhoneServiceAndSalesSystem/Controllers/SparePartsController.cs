using Microsoft.AspNetCore.Mvc;
using MobilePhoneServiceAndSalesSystem.Models.Repositories;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    public class SparePartsController : Controller
    {
        private readonly ISparePartMockRepository _sparePartMockRepository;

        public SparePartsController(ISparePartMockRepository sparePartMockRepository)
        {
            _sparePartMockRepository = sparePartMockRepository;
        }

        public IActionResult Index()
        {
            return View(_sparePartMockRepository.Items);
        }

        public IActionResult Details(int id)
        {
            var sparePart = _sparePartMockRepository.Items.FirstOrDefault(s => s.Id == id);

            if (sparePart is null)
            {
                return NotFound();
            }

            return View(sparePart);
        }
    }
}