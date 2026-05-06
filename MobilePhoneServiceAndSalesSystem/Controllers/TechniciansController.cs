using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("technicians")]
    public class TechniciansController : Controller
    {
        private readonly AppDbContext _dbContext;

        public TechniciansController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("")]
        public IActionResult Index()
        {
            var technicians = _dbContext.Technicians
                .Include(t => t.RepairJobs)
                .ToList();

            return View(technicians);
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
        {
            var technician = _dbContext.Technicians
                .Include(t => t.RepairJobs)
                .FirstOrDefault(t => t.Id == id);

            if (technician is null)
            {
                return NotFound();
            }

            return View(technician);
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(Technician technician)
        {
            if (!ModelState.IsValid)
            {
                return View(technician);
            }

            _dbContext.Technicians.Add(technician);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var technician = _dbContext.Technicians.Find(id);

            if (technician is null)
            {
                return NotFound();
            }

            return View(technician);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id, Technician technician)
        {
            if (id != technician.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(technician);
            }

            _dbContext.Technicians.Update(technician);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}