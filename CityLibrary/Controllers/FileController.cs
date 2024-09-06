using CityLibrary.Models;
using CityLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace CityLibrary.Controllers
{
    public class FileController : Controller
    {
        private readonly FileService _fileService;
        private readonly QueueStorageService _queueStorageService;

        public FileController(FileService fileService, QueueStorageService queueStorageService)
        {
            _fileService = fileService;
            _queueStorageService = queueStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new FileViewModel
            {
                Files = await _fileService.ListFilesAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(FileViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("", "No file selected for upload.");
                model.Files = await _fileService.ListFilesAsync(); // Preserve existing list
                return View("Index", model);
            }

            string tempFilePath = Path.GetTempFileName();
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            await _fileService.UploadFileAsync(tempFilePath, model.File.FileName);
            System.IO.File.Delete(tempFilePath);

            // Send a message to the queue
            await _queueStorageService.SendMessageAsync($"File uploaded: {model.File.FileName}");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            string tempFilePath = Path.GetTempFileName();
            await _fileService.DownloadFileAsync(fileName, tempFilePath);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(tempFilePath);
            System.IO.File.Delete(tempFilePath);

            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            await _fileService.DeleteFileAsync(fileName);

            // Send a message to the queue
            await _queueStorageService.SendMessageAsync($"File deleted: {fileName}");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RecycleBin()
        {
            var model = new FileViewModel
            {
                Files = await _fileService.ListDeletedFilesAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Restore(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            await _fileService.RestoreFileAsync(fileName);

            // Send a message to the queue
            await _queueStorageService.SendMessageAsync($"File restored: {fileName}");

            return RedirectToAction("Index");
        }
    }
}
