using Microsoft.AspNetCore.Mvc;
using MobilePhoneServiceAndSalesSystem.Models.Repositories;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    public class PhonesController : Controller
    {
        private readonly IPhoneMockRepository _phoneMockRepository;

        public PhonesController(IPhoneMockRepository phoneMockRepository)
        {
            _phoneMockRepository = phoneMockRepository;
        }

        public IActionResult Index()
        {
            return View(_phoneMockRepository.Items);
        }

        public IActionResult Details(int id)
        {
            var phone = _phoneMockRepository.Items.FirstOrDefault(p => p.Id == id);

            if (phone is null)
            {
                return NotFound();
            }

            return View(phone);
        }
    }
}