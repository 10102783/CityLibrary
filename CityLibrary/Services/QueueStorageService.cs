using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using CityLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityLibrary.Services
{
    public class QueueStorageService
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly string _queueName;

        public QueueStorageService(string connectionString, string queueName)
        {
            _queueServiceClient = new QueueServiceClient(connectionString);
            _queueName = queueName.ToLower(); // Ensure queue name is in lowercase
        }

        public async Task SendMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be null or whitespace.");
            }

            var queueClient = _queueServiceClient.GetQueueClient(_queueName);
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(message);
        }

        public async Task<IEnumerable<QueueMessageModel>> PeekMessagesAsync(int maxMessages = 32)
        {
            var queueClient = _queueServiceClient.GetQueueClient(_queueName);
            PeekedMessage[] messages = await queueClient.PeekMessagesAsync(maxMessages: maxMessages);

            return messages.Select(m => new QueueMessageModel
            {
                MessageId = m.MessageId,
                MessageText = m.MessageText,
                InsertedOn = m.InsertedOn,
                ExpiresOn = m.ExpiresOn
            });
        }

        public async Task<IEnumerable<QueueMessageModel>> ReceiveMessagesAsync(int maxMessages = 32)
        {
            var queueClient = _queueServiceClient.GetQueueClient(_queueName);
            QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(maxMessages: maxMessages);

            return messages.Select(m => new QueueMessageModel
            {
                MessageId = m.MessageId,
                MessageText = m.MessageText,
                InsertedOn = m.InsertedOn,
                ExpiresOn = m.ExpiresOn,
                PopReceipt = m.PopReceipt
            });
        }

        public async Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(popReceipt))
            {
                throw new ArgumentException("Message ID and Pop Receipt cannot be null or whitespace.");
            }

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(_queueName);

                // Ensure the queue exists
                if (!await queueClient.ExistsAsync())
                {
                    throw new InvalidOperationException("Queue does not exist.");
                }

                // Delete the message
                await queueClient.DeleteMessageAsync(messageId, popReceipt);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "MessageNotFound")
            {
                // Log specific error
                Console.WriteLine($"Message with ID {messageId} not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred while deleting the message: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateMessageAsync(string messageId, string popReceipt, string newMessageText)
        {
            if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(popReceipt) || string.IsNullOrWhiteSpace(newMessageText))
            {
                throw new ArgumentException("Message ID, Pop Receipt, and new message text cannot be null or whitespace.");
            }

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(_queueName);

                // Ensure the queue exists
                if (!await queueClient.ExistsAsync())
                {
                    throw new InvalidOperationException("Queue does not exist.");
                }

                // Update the message
                await queueClient.UpdateMessageAsync(messageId, popReceipt, newMessageText, visibilityTimeout: TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred while updating the message: {ex.Message}");
                throw;
            }
        }
    }
}
