using CityLibrary.Models;
using CityLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CityLibrary.Controllers
{
    public class QueueController : Controller
    {
        private readonly QueueStorageService _queueStorageService;

        public QueueController(QueueStorageService queueStorageService)
        {
            _queueStorageService = queueStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _queueStorageService.PeekMessagesAsync();
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError("", "Message cannot be empty.");
                return RedirectToAction("Index");
            }

            try
            {
                await _queueStorageService.SendMessageAsync(message);
                TempData["SuccessMessage"] = "Message sent successfully.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(string messageId, string popReceipt)
        {
            if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(popReceipt))
            {
                ModelState.AddModelError("", "Invalid message data.");
                return RedirectToAction("Index");
            }

            try
            {
                await _queueStorageService.DeleteMessageAsync(messageId, popReceipt);
                TempData["SuccessMessage"] = "Message deleted successfully.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMessage(string messageId, string popReceipt, string newMessageText)
        {
            if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(popReceipt) || string.IsNullOrWhiteSpace(newMessageText))
            {
                ModelState.AddModelError("", "Invalid message data.");
                return RedirectToAction("Index");
            }

            try
            {
                await _queueStorageService.UpdateMessageAsync(messageId, popReceipt, newMessageText);
                TempData["SuccessMessage"] = "Message updated successfully.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Gallery()
        {
            var messages = await _queueStorageService.ReceiveMessagesAsync(); // Use ReceiveMessagesAsync to get popReceipt
            return View(messages);
        }
    }
}

