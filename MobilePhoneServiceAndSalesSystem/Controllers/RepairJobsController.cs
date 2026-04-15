using Microsoft.AspNetCore.Mvc;
using MobilePhoneServiceAndSalesSystem.Models.Repositories;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    public class RepairJobsController : Controller
    {
        private readonly IRepairJobMockRepository _repairJobMockRepository;

        public RepairJobsController(IRepairJobMockRepository repairJobMockRepository)
        {
            _repairJobMockRepository = repairJobMockRepository;
        }

        public IActionResult Index()
        {
            return View(_repairJobMockRepository.Items);
        }

        [HttpGet]
        public IActionResult Tracker(int? searchId)
        {
            if (searchId.HasValue)
            {
                var repairJob = _repairJobMockRepository.Items.FirstOrDefault(r => r.Id == searchId.Value);
                if (repairJob != null)
                {
                    ViewBag.RepairJob = repairJob;
                }
                else
                {
                    ViewBag.ErrorMessage = $"No repair job found with ID {searchId.Value}.";
                }
            }
            return View();
        }

        public IActionResult Details(int id)
        {
            var repairJob = _repairJobMockRepository.Items.FirstOrDefault(r => r.Id == id);

            if (repairJob is null)
            {
                return NotFound();
            }

            return View(repairJob);
        }
    }
}