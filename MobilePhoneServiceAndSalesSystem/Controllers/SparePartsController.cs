using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("spare-parts")]
    public class SparePartsController : Controller
    {
        private readonly AppDbContext _dbContext;

        public SparePartsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("")]
        public IActionResult Index()
        {
            var spareParts = _dbContext.SpareParts
                .Include(sp => sp.RepairJobs)
                .ToList();

            return View(spareParts);
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
        {
            var sparePart = _dbContext.SpareParts
                .Include(sp => sp.RepairJobs)
                .FirstOrDefault(sp => sp.Id == id);

            if (sparePart is null)
            {
                return NotFound();
            }

            return View(sparePart);
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(SparePart sparePart)
        {
            if (!ModelState.IsValid)
            {
                return View(sparePart);
            }

            _dbContext.SpareParts.Add(sparePart);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var sparePart = _dbContext.SpareParts.Find(id);

            if (sparePart is null)
            {
                return NotFound();
            }

            return View(sparePart);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id, SparePart sparePart)
        {
            if (id != sparePart.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(sparePart);
            }

            _dbContext.SpareParts.Update(sparePart);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}