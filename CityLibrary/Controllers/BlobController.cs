using CityLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CityLibrary.Controllers
{
    public class BlobController : Controller
    {
        private readonly BlobStorageService _blobStorageService;
        private readonly QueueStorageService _queueStorageService;

        public BlobController(BlobStorageService blobStorageService, QueueStorageService queueStorageService)
        {
            _blobStorageService = blobStorageService;
            _queueStorageService = queueStorageService;
        }

        public async Task<IActionResult> Index()
        {
            // Retrieve list of blobs to display on the index page
            var blobs = await _blobStorageService.ListBlobsAsync();
            return View(blobs);
        }

        [HttpPost]
        public async Task<IActionResult> UploadBlob(IFormFile file, string blobName)
        {
            if (file == null || file.Length == 0 || string.IsNullOrEmpty(blobName))
            {
                ModelState.AddModelError("", "Invalid file or blob name.");
                return View("Index");
            }

            using (var stream = file.OpenReadStream())
            {
                await _blobStorageService.UploadBlobAsync(blobName, stream);
            }

            // Send a message to the queue
            await _queueStorageService.SendMessageAsync($"Blob uploaded: {blobName}");

            TempData["SuccessMessage"] = "Uploaded successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadBlob(string blobName)
        {
            if (string.IsNullOrEmpty(blobName))
            {
                ModelState.AddModelError("", "Invalid blob name.");
                return View("Index");
            }

            var blobStream = await _blobStorageService.DownloadBlobAsync(blobName);
            return File(blobStream, "application/octet-stream", blobName);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBlob(string blobName)
        {
            if (string.IsNullOrEmpty(blobName))
            {
                ModelState.AddModelError("", "Invalid blob name.");
                return View("Index");
            }

            await _blobStorageService.DeleteBlobAsync(blobName);

            // Send a message to the queue
            await _queueStorageService.SendMessageAsync($"Blob deleted: {blobName}");

            TempData["SuccessMessage"] = "Deleted successfully.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Gallery()
        {
            // Retrieve list of blobs to display in the gallery view
            var blobs = await _blobStorageService.ListBlobsAsync();
            return View(blobs);
        }
    }
}
