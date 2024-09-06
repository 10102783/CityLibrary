using CityLibrary.Models;
using CityLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CityLibrary.Controllers
{
    public class BorrowerController : Controller
    {
        private readonly BorrowerService _borrowerService;
        private readonly QueueStorageService _queueStorageService;

        public BorrowerController(BorrowerService borrowerService, QueueStorageService queueStorageService)
        {
            _borrowerService = borrowerService;
            _queueStorageService = queueStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var borrowers = await _borrowerService.GetAllBorrowersAsync();
            return View(borrowers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Borrower borrower)
        {
            if (ModelState.IsValid)
            {
                await _borrowerService.AddBorrowerAsync(borrower);

                // Send a message to the queue
                await _queueStorageService.SendMessageAsync($"New borrower created: {borrower.PartitionKey}-{borrower.RowKey}");

                return RedirectToAction("Details", new { partitionKey = borrower.PartitionKey, rowKey = borrower.RowKey });
            }
            return View(borrower);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var borrower = await _borrowerService.GetBorrowerAsync(partitionKey, rowKey);
            if (borrower == null)
            {
                return NotFound();
            }
            borrower.UpdateTotal();

            return View(borrower);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _borrowerService.DeleteBorrowerAsync(partitionKey, rowKey);

            // Send a message to the queue
            await _queueStorageService.SendMessageAsync($"Borrower deleted: {partitionKey}-{rowKey}");

            return RedirectToAction("Index");
        }
    }
}
