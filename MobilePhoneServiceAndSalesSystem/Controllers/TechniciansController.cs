using Microsoft.AspNetCore.Mvc;
using MobilePhoneServiceAndSalesSystem.Models.Repositories;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    public class TechniciansController : Controller
    {
        private readonly ITechnicianMockRepository _technicianMockRepository;

        public TechniciansController(ITechnicianMockRepository technicianMockRepository)
        {
            _technicianMockRepository = technicianMockRepository;
        }

        public IActionResult Index()
        {
            return View(_technicianMockRepository.Items);
        }

        public IActionResult Details(int id)
        {
            var technician = _technicianMockRepository.Items.FirstOrDefault(t => t.Id == id);

            if (technician is null)
            {
                return NotFound();
            }

            return View(technician);
        }
    }
}