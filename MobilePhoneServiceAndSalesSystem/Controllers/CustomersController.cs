using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobilePhoneServiceAndSalesSystem.DAL;
using MobilePhoneServiceAndSalesSystem.Models;
using System.Linq;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("customers")]
    public class CustomersController : Controller
    {
        private readonly AppDbContext _dbContext;

        public CustomersController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("")]
        public IActionResult Index()
        {
            var customers = _dbContext.Customers
                .Include(c => c.Phones)
                .Include(c => c.Orders)
                .ToList();

            return View(customers);
        }

        [Route("{id:int}")]
        public IActionResult Details(int id)
        {
            var customer = _dbContext.Customers
                .Include(c => c.Phones)
                .Include(c => c.Orders)
                .FirstOrDefault(c => c.Id == id);

            if (customer is null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            _dbContext.Customers.Add(customer);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var customer = _dbContext.Customers.Find(id);

            if (customer is null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpPost]
        [Route("edit/{id:int}")]
        public IActionResult Edit(int id, Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            _dbContext.Customers.Update(customer);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}